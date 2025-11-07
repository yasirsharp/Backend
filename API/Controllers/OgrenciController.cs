using Microsoft.AspNetCore.Mvc;
using Business.Abstract;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;

namespace API.Controllers
{
    /// <summary>
    /// Ogrenci Controller
    /// Öğrenci CRUD ve sorgulama operasyonları için API endpoint'leri
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OgrenciController : ControllerBase
    {
        private readonly IOgrenciService _ogrenciService;

        public OgrenciController(IOgrenciService ogrenciService)
        {
            _ogrenciService = ogrenciService;
        }

        /// <summary>
        /// Tüm öğrencileri getirir
        /// </summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _ogrenciService.GetList();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Sayfalanmış, sıralanmış ve filtrelenmiş öğrenci listesi
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası (default: 1)</param>
        /// <param name="pageSize">Sayfa başına kayıt (default: 10, max: 100)</param>
        /// <param name="sortBy">Sıralama alanı (Id, Ad, Soyad, OgrenciNo, BolumId)</param>
        /// <param name="sortOrder">Sıralama yönü (asc, desc)</param>
        /// <param name="searchTerm">Arama terimi (ad, soyad, öğrenci no)</param>
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

            var result = _ogrenciService.GetPagedList(paginationParams);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// ID'ye göre öğrenci getirir
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var result = _ogrenciService.GetById(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// UserId'ye göre öğrenci getirir (1-to-1 relationship)
        /// Token'dan userId alınarak öğrenci bilgileri döndürülür
        /// </summary>
        [HttpGet("by-user-id/{userId}")]
        public IActionResult GetByUserId(int userId)
        {
            var result = _ogrenciService.GetByUserId(userId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        /// <summary>
        /// Öğrenci numarasına göre öğrenci getirir
        /// </summary>
        [HttpGet("by-ogrenci-no/{ogrenciNo}")]
        public IActionResult GetByOgrenciNo(string ogrenciNo)
        {
            var result = _ogrenciService.GetByOgrenciNo(ogrenciNo);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        /// <summary>
        /// Yeni öğrenci ekler
        /// </summary>
        [HttpPost]
        public IActionResult Add(Ogrenci ogrenci)
        {
            var result = _ogrenciService.Add(ogrenci);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        /// <summary>
        /// Öğrenci bilgilerini günceller
        /// </summary>
        [HttpPut]
        public IActionResult Update(Ogrenci ogrenci)
        {
            var result = _ogrenciService.Update(ogrenci);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Öğrenci siler
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var ogrenci = _ogrenciService.GetById(id);
            if (!ogrenci.Success)
                return NotFound(ogrenci);
            
            var result = _ogrenciService.Delete(ogrenci.Data);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
