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

        public UserController(IUserService userService)
        {
            _userService = userService;
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
            user.FirstName = userUpdateDto.FirstName;
            user.LastName = userUpdateDto.LastName;
            user.Email = userUpdateDto.Email;
            user.UserName = userUpdateDto.UserName;
            user.Status = userUpdateDto.Status;

            var result = _userService.Update(user);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
