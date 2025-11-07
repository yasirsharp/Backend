using Microsoft.AspNetCore.Mvc;
using Business.Abstract;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;

namespace API.Controllers
{
    [Route("api/dbap")]
    [ApiController]
    public class DBAPController : ControllerBase
    {
        private readonly IDBAPService _dbapService;

        public DBAPController(IDBAPService dbapService)
        {
            _dbapService = dbapService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _dbapService.GetAll();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("details")]
        public IActionResult GetAllDetails()
        {
            var result = _dbapService.GetAllDetails();
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
            var result = _dbapService.GetById(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("detail/{id}")]
        public IActionResult GetDetail(int id)
        {
            var result = _dbapService.GetDetail(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("bolum/{bolumId}")]
        public IActionResult GetByBolumId(int bolumId)
        {
            var result = _dbapService.GetByBolumId(bolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("bolum/details/{bolumId}")]
        public IActionResult GetDetailsByBolumId(int bolumId)
        {
            var result = _dbapService.GetDetailsByBolumId(bolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost]
        public IActionResult Add(DersBolumAkademikPersonel dersBolumAkademikPersonel)
        {
            var result = _dbapService.Add(dersBolumAkademikPersonel);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpPut]
        public IActionResult Update(DersBolumAkademikPersonel dersBolumAkademikPersonel)
        {
            var result = _dbapService.Update(dersBolumAkademikPersonel);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var dbap = _dbapService.GetById(id).Data;
            if (dbap == null)
                return NotFound(new { Success = false, Message = "DBAP kaydı bulunamadı" });

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