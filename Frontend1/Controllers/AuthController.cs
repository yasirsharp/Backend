using Business.Abstract;
using Entity.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace Frontend1.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            foreach (var item in Request.Cookies)
            {
                if (item.Key == "AuthToken")
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return View();
        }

        [HttpPost("/auth/login")]
        public async Task<IActionResult> Login([FromBody] UserForLoginDto userForLoginDto)
        {
            try
            {
                var userToLogin = _authService.Login(userForLoginDto);
                if (!userToLogin.Success)
                {
                    return BadRequest(userToLogin.Message);
                }

                var result = _authService.CreateAccessToken(userToLogin.Data);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }

                // Token ve kullanıcı bilgilerini cookie olarak kaydet
                Response.Cookies.Append("AuthToken", result.Data.Token, new CookieOptions
                {
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddMinutes(120)
                });

                string claims = "";
                var userClaims = _userService.GetClaims(userToLogin.Data);
                if (userClaims.Success)
                {
                    foreach (var claim in userClaims.Data)
                    {
                        claims += $"{claim.Id}:{claim.Name};";
                    }
                }
                Response.Cookies.Append("UserInfo-Id", userToLogin.Data.Id.ToString(), new CookieOptions
                {
                    
                    
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddMinutes(120)
                });
                Response.Cookies.Append("UserInfo-FullName", $"{userToLogin.Data.FirstName} {userToLogin.Data.LastName}", new CookieOptions
                {
                    
                    
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddMinutes(120)
                });
                Response.Cookies.Append("UserInfo-Email", userToLogin.Data.Email, new CookieOptions
                {
                    
                    
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddMinutes(120)
                });
                Response.Cookies.Append("UserInfo-Status", userToLogin.Data.Status.ToString(), new CookieOptions
                {
                    
                    
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddMinutes(120)
                });
                Response.Cookies.Append("User-Claims", claims, new CookieOptions
                {
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddMinutes(120)
                });

                return Ok(new { message = "Giriş başarılı!" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Bir hata oluştu: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost("/auth/register")]
        public async Task<IActionResult> Register([FromBody] UserForRegisterDto userForRegisterDto)
        {
            try
            {
                var userExists = _authService.UserExists(userForRegisterDto.Email);
                if (!userExists.Success)
                {
                    return BadRequest(userExists.Message);
                }

                var registerResult = _authService.Register(userForRegisterDto, userForRegisterDto.Password);
                if (!registerResult.Success)
                {
                    return BadRequest(registerResult.Message);
                }

                var result = _authService.CreateAccessToken(registerResult.Data);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }

                string claims = "";
                var userClaims = _userService.GetClaims(registerResult.Data);
                if (userClaims.Success)
                {
                    foreach (var claim in userClaims.Data)
                    {
                        claims += $"{claim.Id}:{claim.Name};";
                    }
                }
                Response.Cookies.Append("AuthToken", result.Data.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddMinutes(120)
                });
                Response.Cookies.Append("UserInfo-Id", registerResult.Data.Id.ToString(), new CookieOptions
                {
                    
                    
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddMinutes(120)
                });
                Response.Cookies.Append("UserInfo-FullName", $"{registerResult.Data.FirstName} {registerResult.Data.LastName}", new CookieOptions
                {
                    
                    
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddMinutes(120)
                });
                Response.Cookies.Append("UserInfo-Email", registerResult.Data.Email, new CookieOptions
                {
                    
                    
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddMinutes(120)
                });
                Response.Cookies.Append("UserInfo-Status", registerResult.Data.Status.ToString(), new CookieOptions
                {
                    
                    
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddMinutes(120)
                });
                Response.Cookies.Append("User-Claims", claims, new CookieOptions
                {
                    
                    
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddMinutes(120)
                });

                return Ok(new { message = "Kayıt başarılı!" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Bir hata oluştu: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Tüm cookie'leri sil
                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie);
                }

                // Oturumu sonlandır
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return Ok(new { message = "Çıkış başarılı!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Çıkış yapılırken bir hata oluştu: " + ex.Message);
            }
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
