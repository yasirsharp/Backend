using Microsoft.AspNetCore.Mvc;
using Business.Abstract;
using Entity.DTOs;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
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

        [HttpPost("register")]
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
        /// TEST için öğrenci kayıt endpoint'i
        /// </summary>
        [HttpPost("register-ogrenci")]
        public IActionResult RegisterOgrenci(OgrenciRegisterDto ogrenciRegisterDto)
        {
            var userExists = _authService.UserExists(ogrenciRegisterDto.Email);
            if (!userExists.Success)
                return BadRequest(userExists);

            var registerResult = _authService.RegisterOgrenci(ogrenciRegisterDto);
            if (!registerResult.Success)
                return BadRequest(registerResult);

            var result = _authService.CreateAccessToken(registerResult.Data);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// TEST için akademik personel kayıt endpoint'i
        /// </summary>
        [HttpPost("register-akademik-personel")]
        public IActionResult RegisterAkademikPersonel(AkademikPersonelRegisterDto akademikPersonelRegisterDto)
        {
            var userExists = _authService.UserExists(akademikPersonelRegisterDto.Email);
            if (!userExists.Success)
                return BadRequest(userExists);

            var registerResult = _authService.RegisterAkademikPersonel(akademikPersonelRegisterDto);
            if (!registerResult.Success)
                return BadRequest(registerResult);

            var result = _authService.CreateAccessToken(registerResult.Data);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
    }
} 