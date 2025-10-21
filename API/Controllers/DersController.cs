using Microsoft.AspNetCore.Mvc;
using Business.Abstract;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;
using Entity.DTOs;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DersController : ControllerBase
    {
        private readonly IDersService _dersService;

        public DersController(IDersService dersService)
        {
            _dersService = dersService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _dersService.GetList();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Sayfalanmış, sıralanmış ve filtrelenmiş ders listesi
        /// </summary>
        [HttpGet("paged")]
        public IActionResult GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = "Id",
            [FromQuery] string sortOrder = "asc",
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

            var result = _dersService.GetPagedList(paginationParams);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var result = _dersService.GetById(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost]
        public IActionResult Add(Ders ders)
        {
            var result = _dersService.Add(ders);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpPut]
        public IActionResult Update(Ders ders)
        {
            var result = _dersService.Update(ders);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var ders = _dersService.GetById(id);
            if (!ders.Success)
                return NotFound(ders);
            
            var result = _dersService.Delete(ders.Data);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("with-bolumler")]
        public IActionResult AddWithBolumler(DersEkleDTO dersEkleDto)
        {
            var result = _dersService.AddDersWithBolumler(dersEkleDto);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpGet("with-bolumler")]
        public IActionResult GetAllWithBolumler()
        {
            var result = _dersService.GetAllWithBolumler();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPut("with-bolumler")]
        public IActionResult UpdateWithBolumler(DersGuncelleDTO dersGuncelleDto)
        {
            var result = _dersService.UpdateDersWithBolumler(dersGuncelleDto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("check-kod-unique")]
        public IActionResult CheckKodUnique([FromQuery] string kod, [FromQuery] int? excludeDersId = null)
        {
            var result = _dersService.IsKodUnique(kod, excludeDersId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
} 