using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using Entity.DTOs;
using Core.Extensions;
using API.Security;
using Business.Concrete;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IConfiguration configuration, IUserService userService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _configuration = configuration;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>Gerçek kişi: kişi olarak girilmişse süper yöneticinin Id'si, değilse kullanıcının kendisi</summary>
        private int? RealUserId() =>
            int.TryParse(User.FindFirst(AuthManager.ActAsByClaim)?.Value, out var by) ? by : User.GetUserId();

        private bool IsSuperAdmin(int userId) =>
            _userService.GetClaims(new Core.Entities.Concrete.User { Id = userId }).Data?.Any(c => c.Name == Roles.SuperAdmin) == true;

        /// <summary>
        /// Süper yönetici başka bir kullanıcı olarak girer: o kişinin rolleri ve bölümleriyle token döner. Kişi olarak
        /// girilmişken de başka bir kullanıcıya doğrudan geçilebilir (gerçek kişi yine süper yönetici olmalı).
        /// </summary>
        [HttpPost("impersonate/{userId:int}")]
        public IActionResult Impersonate(int userId)
        {
            var realId = RealUserId();
            if (realId == null)
                return Unauthorized();
            var actor = _userService.GetById(realId.Value).Data;
            var target = _userService.GetById(userId).Data;
            if (target == null)
                return NotFound(new ErrorResult(Messages.UserNotFound));
            var targetRoles = _userService.GetClaims(target).Data?.Select(c => c.Name) ?? Enumerable.Empty<string>();
            var error = ImpersonationRules.Check(actor != null && actor.Status && IsSuperAdmin(actor.Id), realId.Value, userId, target.Status, targetRoles);
            if (error != null)
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResult(error));

            var token = _authService.CreateImpersonationToken(target, actor!);
            if (!token.Success)
                return BadRequest(token);
            _logger.LogWarning("Kişi olarak giriş: {Actor} (#{ActorId}) → {Target} (#{TargetId})",
                $"{actor!.FirstName} {actor.LastName}", actor.Id, $"{target.FirstName} {target.LastName}", target.Id);
            return Ok(new SuccessDataResult<Core.Utilities.Security.JWT.AccessToken>(token.Data, $"{target.FirstName} {target.LastName} olarak giriş yapıldı."));
        }

        /// <summary>Kişi olarak girilmiş oturumdan süper yöneticinin kendi hesabına (parolasız) döner</summary>
        [HttpPost("stop-impersonation")]
        public IActionResult StopImpersonation()
        {
            if (!int.TryParse(User.FindFirst(AuthManager.ActAsByClaim)?.Value, out var actorId))
                return BadRequest(new ErrorResult("Kişi olarak girilmiş bir oturum değil."));
            var actor = _userService.GetById(actorId).Data;
            if (actor == null || !actor.Status || !IsSuperAdmin(actorId))
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResult("Süper yönetici hesabı artık geçerli değil; yeniden giriş yapın."));
            var token = _authService.CreateAccessToken(actor);
            if (!token.Success)
                return BadRequest(token);
            _logger.LogWarning("Kişi olarak giriş sona erdi: {Actor} (#{ActorId}) ← #{TargetId}",
                $"{actor.FirstName} {actor.LastName}", actor.Id, User.GetUserId());
            return Ok(new SuccessDataResult<Core.Utilities.Security.JWT.AccessToken>(token.Data, "Kendi hesabınıza döndünüz."));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public IActionResult Login(UserForLoginDto userForLoginDto)
        {
            var userToLogin = _authService.Login(userForLoginDto);
            if (!userToLogin.Success)
                return BadRequest(userToLogin);

            var result = _authService.CreateAccessToken(userToLogin.Data);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Genel kayıt. Varsayılan olarak kapalıdır (Auth:AllowPublicRegistration=false): hesapları yönetici açar.
        /// Açıksa kayıt olan kullanıcıya hiçbir rol atanmaz; rol ataması yönetici tarafından yapılır.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public IActionResult Register(UserForRegisterDto userForRegisterDto)
        {
            if (!_configuration.GetValue<bool>("Auth:AllowPublicRegistration"))
                return NotFound();

            var userExists = _authService.UserExists(userForRegisterDto.Email);
            if (!userExists.Success)
                return BadRequest(userExists);

            var registerResult = _authService.Register(userForRegisterDto, userForRegisterDto.Password);
            if (!registerResult.Success)
                return BadRequest(registerResult);

            var result = _authService.CreateAccessToken(registerResult.Data);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Öğrenci hesabı oluşturur (sadece yönetici). Önceden herkese açıktı ve istemciden gelen
        /// bölüm bilgisiyle "ogrenci" rolü veriyordu.
        /// </summary>
        [HttpPost("register-ogrenci")]
        [Authorize(Roles = Roles.Yonetici)]
        public async Task<IActionResult> RegisterOgrenci(OgrenciRegisterDto ogrenciRegisterDto)
        {
            var result = await _authService.RegisterOgrenciAsync(ogrenciRegisterDto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Akademik personel hesabı oluşturur (sadece yönetici). Önceden herkese açıktı ve
        /// çağıran herkese "akademik.personel" rolü veriyordu.
        /// </summary>
        [HttpPost("register-akademik-personel")]
        [Authorize(Roles = Roles.Yonetici)]
        public async Task<IActionResult> RegisterAkademikPersonel(AkademikPersonelRegisterDto akademikPersonelRegisterDto)
        {
            var userExists = _authService.UserExists(akademikPersonelRegisterDto.Email);
            if (!userExists.Success)
                return BadRequest(userExists);

            var registerResult = await _authService.RegisterAkademikPersonelAsync(akademikPersonelRegisterDto);
            if (!registerResult.Success)
                return BadRequest(registerResult);

            return Ok(new SuccessDataResult<UserDto>(UserDto.FromEntity(registerResult.Data), registerResult.Message));
        }

        /// <summary>
        /// Giriş yapmış kullanıcının parolasını değiştirir ve yeni bir token döner. Geçici parolayla giriş yapan
        /// kullanıcının ("pwd_change" claim'i) kullanabildiği birkaç uçtan biridir.
        /// </summary>
        [HttpPost("change-password")]
        [EnableRateLimiting("auth")]
        public IActionResult ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();
            if (User.HasClaim(c => c.Type == AuthManager.ActAsByClaim))
                return StatusCode(StatusCodes.Status403Forbidden,
                    new ErrorResult("Kullanıcı olarak girilmişken parola değiştirilemez. Kendi hesabınıza dönün."));

            var result = _authService.ChangePassword(userId.Value, changePasswordDto);
            if (!result.Success)
                return BadRequest(result);

            var token = _authService.CreateAccessToken(result.Data);
            if (!token.Success)
                return BadRequest(token);

            return Ok(new SuccessDataResult<Core.Utilities.Security.JWT.AccessToken>(token.Data, result.Message));
        }
    }
}
