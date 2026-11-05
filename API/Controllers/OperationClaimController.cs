using Microsoft.AspNetCore.Mvc;
using Business.Constants;
using Microsoft.AspNetCore.Authorization;
using Business.Abstract;
using Core.Entities.Concrete;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.Yonetici)]
    public class OperationClaimController : ControllerBase
    {
        private readonly IOperationClaimService _operationClaimService;

        public OperationClaimController(IOperationClaimService operationClaimService)
        {
            _operationClaimService = operationClaimService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _operationClaimService.GetAll();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var result = _operationClaimService.GetById(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        public IActionResult Add(OperationClaim operationClaim)
        {
            var result = _operationClaimService.Add(operationClaim);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpPut]
        [Authorize(Roles = Roles.SuperAdmin)]
        public IActionResult Update(OperationClaim operationClaim)
        {
            var result = _operationClaimService.Update(operationClaim);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete]
        [Authorize(Roles = Roles.SuperAdmin)]
        public IActionResult Delete(OperationClaim operationClaim)
        {
            var result = _operationClaimService.Delete(operationClaim);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
} 