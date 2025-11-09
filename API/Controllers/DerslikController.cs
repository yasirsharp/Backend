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

        /// <summary>
        /// Sayfalanmış derslik listesi (bölümlerle birlikte)
        /// </summary>
        [HttpGet("with-bolumler/paged")]
        public IActionResult GetPagedWithBolumler(
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

            // Tüm derslikleri bölümlerle birlikte getir
            var allResult = _derslikService.GetAllWithBolumler();
            if (!allResult.Success)
                return BadRequest(allResult);

            var allDerslikler = allResult.Data;

            // Frontend'den gelen arama terimine göre filtrele
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                allDerslikler = allDerslikler
                    .Where(d => d.DerslikAd.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Sıralama
            allDerslikler = sortBy?.ToLower() switch
            {
                "ad" or "derslikad" => sortOrder.ToLower() == "desc"
                    ? allDerslikler.OrderByDescending(d => d.DerslikAd).ToList()
                    : allDerslikler.OrderBy(d => d.DerslikAd).ToList(),
                "kapasite" => sortOrder.ToLower() == "desc"
                    ? allDerslikler.OrderByDescending(d => d.Kapasite).ToList()
                    : allDerslikler.OrderBy(d => d.Kapasite).ToList(),
                _ => sortOrder.ToLower() == "desc"
                    ? allDerslikler.OrderByDescending(d => d.DerslikId).ToList()
                    : allDerslikler.OrderBy(d => d.DerslikId).ToList()
            };

            // Pagination
            var totalItems = allDerslikler.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var pagedItems = allDerslikler
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagedResult = new
            {
                Data = pagedItems,
                Success = true,
                Message = $"{pagedItems.Count} derslik bulundu (Toplam: {totalItems})",
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };

            return Ok(pagedResult);
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

        /// <summary>
        /// Sadece belirtilen bölümden derslik ilişkisini sil
        /// </summary>
        [HttpDelete("{derslikId}/bolum/{bolumId}")]
        public IActionResult RemoveFromBolum(int derslikId, int bolumId)
        {
            var result = _derslikService.RemoveDerslikFromBolum(derslikId, bolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
} 