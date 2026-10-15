using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using Entity.DTOs;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
        /// Genel kayıt. Kayıt olan kullanıcıya hiçbir rol atanmaz; rol ataması yönetici tarafından yapılır.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public IActionResult Register(UserForRegisterDto userForRegisterDto)
        {
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
        public IActionResult RegisterOgrenci(OgrenciRegisterDto ogrenciRegisterDto)
        {
            var userExists = _authService.UserExists(ogrenciRegisterDto.Email);
            if (!userExists.Success)
                return BadRequest(userExists);

            var registerResult = _authService.RegisterOgrenci(ogrenciRegisterDto);
            if (!registerResult.Success)
                return BadRequest(registerResult);

            return Ok(new SuccessDataResult<UserDto>(UserDto.FromEntity(registerResult.Data), registerResult.Message));
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
    }
}
