using Business.Abstract;
using Core.Entities.Concrete;
using Microsoft.AspNetCore.Mvc;

namespace Frontend1.Controllers
{
    public class ManagementController : Controller
    {
        private IUserService _userService;
        private IOperationClaimService _operationClaimService;
        private IUserOperationClaimService _userOperationClaimService;

        public ManagementController(IUserService userService, IOperationClaimService operationClaimService, IUserOperationClaimService userOperationClaimService)
        {
            _userService = userService;
            _operationClaimService = operationClaimService;
            _userOperationClaimService = userOperationClaimService;
        }

        public IActionResult User()
        {
            ViewData["Users"] = _userService.GetAll();
            ViewData["OperationClaims"] = _operationClaimService.GetAll();
            ViewData["UserOperationClaims"] = _userOperationClaimService.GetAll();

            return View();
        }

        [HttpGet]
        [Route("/management/operationclaim/getall")]
        public IActionResult OperationClaimGetAll()
        {
            return Json(_operationClaimService.GetAll());
        }
        [HttpGet]
        [Route("/management/operationclaim/getbyid/{id}")]
        public IActionResult OperationClaimGetById(int id)
        {
            return Json(_operationClaimService.GetById(id));
        }
        [HttpGet]
        [Route("/management/useroperationclaim/getall")]
        public IActionResult UserOperationClaimAll()
        {
            return Json(_userOperationClaimService.GetAll());
        }

        [HttpGet]
        [Route("/management/user/getall")]
        public IActionResult UserAll()
        {
            return Json(_userService.GetAll());
        }
        [HttpGet]
        [Route("/management/user/getbyid/{id}")]
        public IActionResult UserGetById(int id)
        {
            return Json(_userService.GetById(id));
        }

        [HttpGet]
        [Route("/management/operationclaim/delete/{id}")]
        public IActionResult OperationClaimDelete(int id)
        {
            var result = _operationClaimService.GetById(id);
            if (result == null) return BadRequest();

            try
            {
                _operationClaimService.Delete(result.Data);
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }

            return Ok();
        }
        [HttpPost]
        [Route("/management/operationclaim/update")]
        public IActionResult OperationClaimDelete(OperationClaim operationClaim)
        {
            var result = _operationClaimService.GetById(operationClaim.Id);
            if (result == null) return BadRequest();

            try
            {
                _operationClaimService.Update(operationClaim);
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }

            return Ok();
        }
        [HttpPost]
        [Route("/management/operationclaim/add")]
        public IActionResult OperationClaimAdd(OperationClaim operationClaim)
        {
            try
            {
                _operationClaimService.Add(operationClaim);
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }

            return Ok();
        }
        [HttpPost]
        [Route("/management/useroperationclaim/add")]
        public IActionResult UserOperationClaimAdd([FromBody] UserOperationClaim userOperationClaim)
        {
            try
            {
                var result = _userOperationClaimService.Add(userOperationClaim);
                return Ok(result);
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }

            return Ok();
        }
        [HttpGet]
        [Route("management/useroperationclaim/delete/{id}")]
        public IActionResult DeleteUserOperationClaim(int id)
        {
            var userOperationClaim = new UserOperationClaim { Id = id };
            var result = _userOperationClaimService.Delete(userOperationClaim);
            
            if (result.Success)
                return Ok(result);

            return BadRequest(result.Message);
        }
        [HttpPost]
        [Route("/management/useroperationclaim/update")]
        public IActionResult UserOperationClaimDelete(UserOperationClaim userOperationClaim)
        {
            var result = _userOperationClaimService.GetById(userOperationClaim.Id);
            if (result == null) return BadRequest();

            try
            {
                _userOperationClaimService.Update(userOperationClaim);
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }

            return Ok();
        }

        [HttpGet]
        [Route("/management/user/delete/{id}")]
        public IActionResult UserDelete(int id)
        {
            var result = _userService.Delete(new User { Id = id });
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        public IActionResult Settings()
        {
            return View();
        }
    }
}
