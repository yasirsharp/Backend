using Microsoft.AspNetCore.Mvc;
using Business.Abstract;
using Core.Entities.Concrete;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var result = _userService.GetById(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("email")]
        public IActionResult GetByEmail(string email)
        {
            var result = _userService.GetByMail(email);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("username")]
        public IActionResult GetByUserName(string userName)
        {
            var result = _userService.GetByUserName(userName);
            if (result.Success)
                return Ok(result);
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

        [HttpPost]
        public IActionResult Add(User user)
        {
            var result = _userService.Add(user);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = _userService.GetById(id);
            if (!user.Success || user.Data == null)
                return NotFound(new { Success = false, Message = "Kullanıcı bulunamadı" });

            var result = _userService.Delete(user.Data);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPut]
        public IActionResult Update(User user)
        {
            var result = _userService.Update(user);
            if (!result.Success) BadRequest(result);

            return Ok(result);
        }
    }
} 