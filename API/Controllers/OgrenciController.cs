using Microsoft.AspNetCore.Mvc;
using Business.Constants;
using Microsoft.AspNetCore.Authorization;
using Business.Abstract;
using Core.Extensions;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;
using Entity.DTOs;

namespace API.Controllers
{
    /// <summary>
    /// Ogrenci Controller
    /// Öğrenci CRUD ve sorgulama operasyonları için API endpoint'leri
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        [Authorize(Roles = Roles.Personel)]
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
        [Authorize(Roles = Roles.Personel)]
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
        [Authorize(Roles = Roles.Personel)]
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
            // Öğrenci sadece kendi kaydını görebilir; personel herkesinkini görebilir
            if (User.GetUserId() != userId && !User.IsInAnyRole(Roles.Personel))
                return Forbid();

            var result = _ogrenciService.GetByUserId(userId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        /// <summary>
        /// Öğrenci numarasına göre öğrenci getirir
        /// </summary>
        [HttpGet("by-ogrenci-no/{ogrenciNo}")]
        [Authorize(Roles = Roles.Personel)]
        public IActionResult GetByOgrenciNo(string ogrenciNo)
        {
            var result = _ogrenciService.GetByOgrenciNo(ogrenciNo);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        /// <summary>
        /// Yeni öğrenci ekler: kullanıcı hesabı (kullanıcı adı = öğrenci no) ve geçici parola oluşturulur.
        /// Geçici parola yanıtta bir kez döner; öğrenci ilk girişte parolasını değiştirir.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = Roles.Yonetici)]
        public async Task<IActionResult> Add(OgrenciCreateDto dto)
        {
            var result = await _ogrenciService.CreateAsync(dto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Toplu öğrenci ekleme (en fazla 1000 satır). Her satır ayrı kaydedilir; yanıt satır bazında sonuç
        /// ve başarılı satırların geçici parolalarını içerir.
        /// </summary>
        [HttpPost("bulk")]
        [Authorize(Roles = Roles.Yonetici)]
        [RequestSizeLimit(2_000_000)]
        public async Task<IActionResult> BulkAdd(List<OgrenciCreateDto> rows)
        {
            var result = await _ogrenciService.BulkCreateAsync(rows);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Öğrenci bilgilerini günceller (ad/soyad kullanıcı hesabına da yansır).
        /// </summary>
        [HttpPut]
        [Authorize(Roles = Roles.Yonetici)]
        public async Task<IActionResult> Update(OgrenciUpdateDto dto)
        {
            var result = await _ogrenciService.UpdateAsync(dto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Öğrenci kaydını siler; kullanıcı hesabı pasifleştirilir.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Yonetici)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _ogrenciService.DeleteAsync(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
