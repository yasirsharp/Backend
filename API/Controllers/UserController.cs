using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Abstract;
using Business.Constants;
using Core.Extensions;
using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using Entity.DTOs;

namespace API.Controllers
{
    /// <summary>
    /// Kullanıcı yönetimi. Yanıtlarda User entity'si yerine UserDto döner (PasswordHash/PasswordSalt sızmaz).
    /// Kullanıcı oluşturma /api/Auth/register* uç noktaları üzerinden yapılır.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.Yonetici)]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public UserController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        /// <summary>Kullanıcı yönetimi ekranı: tüm kullanıcılar rol adlarıyla.</summary>
        [HttpGet("with-roles")]
        public IActionResult GetAllWithRoles()
        {
            var result = _userService.GetAllWithRoles();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Geçici parola üretir (yanıtta bir kez gösterilir); kullanıcı ilk girişte yeni parola belirler.
        /// </summary>
        [HttpPost("{id}/reset-password")]
        public IActionResult ResetPassword(int id)
        {
            if (User.GetUserId() == id)
                return BadRequest(new ErrorResult("Kendi parolanızı \"Parolamı değiştir\" sayfasından değiştirin."));

            var result = _authService.ResetPassword(id, User.IsInRole(Roles.SuperAdmin));
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>super.admin hesapları yalnızca super.admin tarafından değiştirilebilir/silinebilir.</summary>
        private bool IsProtectedFromCaller(Core.Entities.Concrete.User target)
        {
            if (User.IsInRole(Roles.SuperAdmin))
                return false;
            var claims = _userService.GetClaims(target).Data;
            return claims != null && claims.Any(c => c.Name == Roles.SuperAdmin);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _userService.GetAll();
            if (result.Success)
                return Ok(new SuccessDataResult<List<UserDto>>(result.Data.Select(UserDto.FromEntity).ToList(), result.Message));
            return BadRequest(result);
        }

        [HttpGet("paged")]
        public IActionResult GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? searchTerm = null)
        {
            var paginationParams = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder,
                SearchTerm = searchTerm
            };

            var result = _userService.GetPagedList(paginationParams);
            if (!result.Success)
                return BadRequest(result);

            var paged = result.Data;
            var dtoPage = new PagedResult<UserDto>
            {
                Items = paged.Items.Select(UserDto.FromEntity).ToList(),
                TotalCount = paged.TotalCount,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize,
                SortBy = paged.SortBy,
                SortOrder = paged.SortOrder,
                SearchTerm = paged.SearchTerm
            };
            return Ok(new SuccessDataResult<PagedResult<UserDto>>(dtoPage, result.Message));
        }

        /// <summary>
        /// Giriş yapmış kullanıcının kendi bilgileri (tüm roller).
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetMe()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = _userService.GetById(userId.Value);
            if (result.Success)
                return Ok(new SuccessDataResult<UserDto>(UserDto.FromEntity(result.Data), result.Message));
            return NotFound(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var result = _userService.GetById(id);
            if (result.Success)
                return Ok(new SuccessDataResult<UserDto>(UserDto.FromEntity(result.Data), result.Message));
            return BadRequest(result);
        }

        [HttpGet("email")]
        public IActionResult GetByEmail(string email)
        {
            var result = _userService.GetByMail(email);
            if (result.Success)
                return Ok(new SuccessDataResult<UserDto>(UserDto.FromEntity(result.Data), result.Message));
            return BadRequest(result);
        }

        [HttpGet("username")]
        public IActionResult GetByUserName(string userName)
        {
            var result = _userService.GetByUserName(userName);
            if (result.Success)
                return Ok(new SuccessDataResult<UserDto>(UserDto.FromEntity(result.Data), result.Message));
            return BadRequest(result);
        }

        [HttpGet("claims/{id}")]
        public IActionResult GetClaims(int id)
        {
            var userResult = _userService.GetById(id);
            if (!userResult.Success)
                return BadRequest(userResult);

            var result = _userService.GetClaims(userResult.Data);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("bolumids/{userId}")]
        public IActionResult GetBolumIds(int userId)
        {
            var result = _userService.GetBolumIds(userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (User.GetUserId() == id)
                return BadRequest(new { Success = false, Message = "Kendi hesabınızı silemezsiniz." });

            var user = _userService.GetById(id);
            if (!user.Success || user.Data == null)
                return NotFound(new { Success = false, Message = "Kullanıcı bulunamadı" });
            if (IsProtectedFromCaller(user.Data))
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResult("super.admin hesabını yalnızca super.admin silebilir."));

            var result = _userService.Delete(user.Data);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Sadece izin verilen alanları günceller. Önceden User entity'si doğrudan alındığı için
        /// PasswordHash/PasswordSalt dahil her alan istemciden değiştirilebiliyordu.
        /// </summary>
        [HttpPut]
        public IActionResult Update(UserUpdateDto userUpdateDto)
        {
            var existing = _userService.GetById(userUpdateDto.Id);
            if (!existing.Success || existing.Data == null)
                return NotFound(new { Success = false, Message = "Kullanıcı bulunamadı" });

            var user = existing.Data;
            if (IsProtectedFromCaller(user))
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResult("super.admin hesabını yalnızca super.admin değiştirebilir."));
            if (User.GetUserId() == user.Id && !userUpdateDto.Status)
                return BadRequest(new ErrorResult("Kendi hesabınızı pasifleştiremezsiniz."));

            var email = (userUpdateDto.Email ?? "").Trim().ToLowerInvariant();
            var userName = (userUpdateDto.UserName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(userUpdateDto.FirstName) || string.IsNullOrWhiteSpace(userUpdateDto.LastName) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(userName))
                return BadRequest(new ErrorResult("Ad, soyad, e-posta ve kullanıcı adı boş olamaz."));

            var byEmail = _userService.GetByMail(email).Data;
            if (byEmail != null && byEmail.Id != user.Id)
                return BadRequest(new ErrorResult("Bu e-posta adresi başka bir kullanıcıda kayıtlı."));
            var byUserName = _userService.GetByUserName(userName).Data;
            if (byUserName != null && byUserName.Id != user.Id)
                return BadRequest(new ErrorResult("Bu kullanıcı adı başka bir kullanıcıda kayıtlı."));

            userUpdateDto.Email = email;
            userUpdateDto.UserName = userName;
            user.FirstName = userUpdateDto.FirstName;
            user.LastName = userUpdateDto.LastName;
            user.Email = userUpdateDto.Email;
            user.UserName = userUpdateDto.UserName;
            user.Status = userUpdateDto.Status;
            user.UpdatedDate = DateTime.Now;

            var result = _userService.Update(user);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
