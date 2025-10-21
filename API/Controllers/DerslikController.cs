using Microsoft.AspNetCore.Mvc;
using Business.Abstract;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;
using Entity.DTOs;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DerslikController : ControllerBase
    {
        private readonly IDerslikService _derslikService;

        public DerslikController(IDerslikService derslikService)
        {
            _derslikService = derslikService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _derslikService.GetList();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Sayfalanmış, sıralanmış ve filtrelenmiş derslik listesi
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

            var result = _derslikService.GetPagedList(paginationParams);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("with-bolumler")]
        public IActionResult GetAllWithBolumler()
        {
            var result = _derslikService.GetAllWithBolumler();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var result = _derslikService.GetById(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost]
        public IActionResult Add(Derslik derslik)
        {
            var result = _derslikService.Add(derslik);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpPost("with-bolumler")]
        public IActionResult AddWithBolumler([FromBody] DerslikEkleDTO derslikEkleDto)
        {
            var result = _derslikService.AddDerslikWithBolumler(derslikEkleDto);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpPut]
        public IActionResult Update(Derslik derslik)
        {
            var result = _derslikService.Update(derslik);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPut("with-bolumler")]
        public IActionResult UpdateWithBolumler([FromBody] DerslikGuncelleDTO derslikGuncelleDto)
        {
            var result = _derslikService.UpdateDerslikWithBolumler(derslikGuncelleDto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var derslik = _derslikService.GetById(id).Data;
            if (derslik == null)
                return NotFound(new { Success = false, Message = "Derslik bulunamadı" });
                
            var result = _derslikService.Delete(derslik);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
} 