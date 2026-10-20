using Microsoft.AspNetCore.Mvc;
using Business.Constants;
using Microsoft.AspNetCore.Authorization;
using Business.Abstract;
using Core.Entities.Concrete;

namespace API.Controllers
{
    /// <summary>
    /// Kullanıcı-rol atamaları. Önceden kimlik doğrulaması yoktu ve herkes kendine super.admin
    /// rolü atayabiliyordu. Artık sadece yöneticiler erişebilir; super.admin rolünü sadece
    /// super.admin atayabilir/kaldırabilir.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.Yonetici)]
    public class UserOperationClaimController : ControllerBase
    {
        private readonly IUserOperationClaimService _userOperationClaimService;
        private readonly IOperationClaimService _operationClaimService;

        public UserOperationClaimController(
            IUserOperationClaimService userOperationClaimService,
            IOperationClaimService operationClaimService)
        {
            _userOperationClaimService = userOperationClaimService;
            _operationClaimService = operationClaimService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _userOperationClaimService.GetAll();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var result = _userOperationClaimService.GetById(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetByUserId(int userId)
        {
            var result = _userOperationClaimService.GetByUserId(userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost]
        public IActionResult Add(UserOperationClaim userOperationClaim)
        {
            if (!CanManageClaim(userOperationClaim.OperationClaimId))
                return Forbid();

            var result = _userOperationClaimService.Add(userOperationClaim);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpPut]
        public IActionResult Update(UserOperationClaim userOperationClaim)
        {
            var existing = _userOperationClaimService.GetById(userOperationClaim.Id);
            if (!existing.Success || existing.Data == null)
                return NotFound(existing);

            if (!CanManageClaim(existing.Data.OperationClaimId) || !CanManageClaim(userOperationClaim.OperationClaimId))
                return Forbid();

            var result = _userOperationClaimService.Update(userOperationClaim);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete]
        public IActionResult Delete(UserOperationClaim userOperationClaim)
        {
            var existing = _userOperationClaimService.GetById(userOperationClaim.Id);
            if (!existing.Success || existing.Data == null)
                return NotFound(existing);

            if (!CanManageClaim(existing.Data.OperationClaimId))
                return Forbid();

            var result = _userOperationClaimService.Delete(existing.Data);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        private bool CanManageClaim(int operationClaimId)
        {
            if (User.IsInRole(Roles.SuperAdmin))
                return true;

            var claim = _operationClaimService.GetById(operationClaimId);
            return claim.Success && claim.Data != null && claim.Data.Name != Roles.SuperAdmin;
        }
    }
}
