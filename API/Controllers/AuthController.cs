using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using Entity.DTOs;
using Core.Extensions;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
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
