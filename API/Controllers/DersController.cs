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

        /// <summary>
        /// Belirli bir bölüme ait dersleri getirir (GorevliLayout için)
        /// </summary>
        [HttpGet("bolum/{bolumId}")]
        public IActionResult GetByBolumId(int bolumId)
        {
            // Bölüme ait tüm DBAP kayıtlarından ders ID'lerini çek
            // Bu işlem için service layer'da yeni metod gerekebilir
            // Şimdilik DersWithBolumler'i kullanabiliriz
            var allDersler = _dersService.GetAllWithBolumler();
            if (!allDersler.Success)
                return BadRequest(allDersler);

            // BolumId'ye göre filtrele
            var filteredDersler = allDersler.Data
                .Where(d => d.Bolumler.Any(b => b.BolumId == bolumId))
                .ToList();

            return Ok(new Core.Utilities.Results.SuccessDataResult<List<DersWithBolumlerDTO>>(
                filteredDersler,
                $"{filteredDersler.Count} ders bulundu."
            ));
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

        /// <summary>
        /// Ders-Bölüm eşleştirmesini siler. Eğer son eşleştirme ise dersi de siler.
        /// </summary>
        [HttpDelete("remove-bolum-mapping/{dersId}/{bolumId}")]
        public IActionResult RemoveDersBolumMapping(int dersId, int bolumId)
        {
            var result = _dersService.RemoveDersBolumMapping(dersId, bolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
} 