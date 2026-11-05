using Microsoft.AspNetCore.Mvc;
using Business.Constants;
using Microsoft.AspNetCore.Authorization;
using Business.Abstract;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;
using Entity.DTOs;
using API.Security;
using Core.Utilities.Results;

namespace API.Controllers
{
    [Route("api/dbap")]
    [ApiController]
    [Authorize]
    public class DBAPController : ControllerBase
    {
        private readonly IDBAPService _dbapService;
        private readonly BolumScope _scope;

        public DBAPController(IDBAPService dbapService, BolumScope scope)
        {
            _dbapService = dbapService;
            _scope = scope;
        }

        private IActionResult OwnBolumOnly() => StatusCode(StatusCodes.Status403Forbidden, new ErrorResult(BolumScope.OwnBolumOnly));

        [HttpGet]
        public IActionResult GetAll()
        {
            if (_scope.IsScoped)
                return OwnBolumOnly();
            var result = _dbapService.GetAll();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("details")]
        public IActionResult GetAllDetails()
        {
            var result = _dbapService.GetAllDetails();
            if (result.Success && _scope.IsScoped)
            {
                // Görevli: kendi bölümlerinin atamaları + kendisinin verdiği dersler (başka bölümde de olsa)
                var me = _scope.PersonelId;
                var list = result.Data.Where(x => _scope.CanAccess(x.BolumId) || x.AkademikPersonelId == me).ToList();
                return Ok(new SuccessDataResult<List<DersBolumAkademikPersonelDTO>>(list, result.Message));
            }
            if (result.Success)
                return Ok(result);
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
            if (_scope.IsScoped)
                return OwnBolumOnly();
            var paginationParams = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder,
                SearchTerm = searchTerm
            };

            var result = _dbapService.GetPagedList(paginationParams);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            if (_scope.IsScoped && !(BolumScope.BolumOfDbap(id) is { } bolum && _scope.CanAccess(bolum)))
                return OwnBolumOnly();
            var result = _dbapService.GetById(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("detail/{id}")]
        public IActionResult GetDetail(int id)
        {
            if (_scope.IsScoped && !(BolumScope.BolumOfDbap(id) is { } bolum && _scope.CanAccess(bolum)))
                return OwnBolumOnly();
            var result = _dbapService.GetDetail(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("bolum/{bolumId}")]
        public IActionResult GetByBolumId(int bolumId)
        {
            if (!_scope.CanAccess(bolumId))
                return OwnBolumOnly();
            var result = _dbapService.GetByBolumId(bolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("bolum/details/{bolumId}")]
        public IActionResult GetDetailsByBolumId(int bolumId)
        {
            if (!_scope.CanAccess(bolumId))
                return OwnBolumOnly();
            var result = _dbapService.GetDetailsByBolumId(bolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult Add(DersBolumAkademikPersonel dersBolumAkademikPersonel)
        {
            if (!_scope.CanWrite(dersBolumAkademikPersonel.BolumId))
                return OwnBolumOnly();
            var result = _dbapService.Add(dersBolumAkademikPersonel);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpPut]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult Update(DersBolumAkademikPersonel dersBolumAkademikPersonel)
        {
            // Hem şimdiki hem yeni bölüm kendi bölümü olmalı
            if (_scope.IsScoped && !(BolumScope.BolumOfDbap(dersBolumAkademikPersonel.Id) is { } eski && _scope.CanWrite(eski)
                                     && _scope.CanWrite(dersBolumAkademikPersonel.BolumId)))
                return OwnBolumOnly();
            var result = _dbapService.Update(dersBolumAkademikPersonel);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult Delete(int id)
        {
            var dbap = _dbapService.GetById(id).Data;
            if (dbap == null)
                return NotFound(new { Success = false, Message = "DBAP kaydı bulunamadı" });
            if (!_scope.CanWrite(dbap.BolumId))
                return OwnBolumOnly();

            var result = _dbapService.Delete(dbap);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("my-courses")]
        public IActionResult GetMyCourses()
        {
            // Token'dan UserId al
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { Success = false, Message = "Kullanıcı kimliği alınamadı" });
            }

            var result = _dbapService.GetMyCoursesForUser(userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
} 