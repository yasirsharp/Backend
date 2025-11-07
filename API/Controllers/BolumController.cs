using Microsoft.AspNetCore.Mvc;
using Business.Abstract;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BolumController : ControllerBase
    {
        private readonly IBolumService _bolumService;

        public BolumController(IBolumService bolumService)
        {
            _bolumService = bolumService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _bolumService.GetList();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Sayfalanmış, sıralanmış ve filtrelenmiş bölüm listesi
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası (default: 1)</param>
        /// <param name="pageSize">Sayfa başına kayıt (default: 10, max: 100)</param>
        /// <param name="sortBy">Sıralama alanı (Id, Ad, CreatedDate, UpdatedDate)</param>
        /// <param name="sortOrder">Sıralama yönü (asc, desc)</param>
        /// <param name="searchTerm">Arama terimi (bölüm adında arar)</param>
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

            var result = _bolumService.GetPagedList(paginationParams);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var result = _bolumService.GetById(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Birden fazla bölüm ID'sine göre bölümleri getirir
        /// </summary>
        /// <param name="ids">Virgülle ayrılmış bölüm ID'leri (örn: 1,2,3)</param>
        [HttpGet("list")]
        public IActionResult GetByIds([FromQuery] string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return BadRequest(new { Success = false, Message = "Bölüm ID'leri boş olamaz" });
            }

            try
            {
                var idList = ids.Split(',').Select(int.Parse).ToList();
                var result = _bolumService.GetByIds(idList);
                
                if (result.Success)
                    return Ok(result);
                return BadRequest(result);
            }
            catch (FormatException)
            {
                return BadRequest(new { Success = false, Message = "Geçersiz bölüm ID formatı" });
            }
        }

        [HttpPost]
        public IActionResult Add(Bolum bolum)
        {
            var result = _bolumService.Add(bolum);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpPut]
        public IActionResult Update(Bolum bolum)
        {
            var result = _bolumService.Update(bolum);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var bolum = _bolumService.GetById(id);
            if (!bolum.Success)
                return NotFound(bolum);
            
            var result = _bolumService.Delete(bolum.Data);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
} 