using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Abstract;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        /// <summary>
        /// Tüm duyuruları getirir (Admin için)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            var result = _announcementService.GetList();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Sayfalanmış duyuru listesi
        /// </summary>
        [HttpGet("paged")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = "CreatedDate",
            [FromQuery] string sortOrder = "desc",
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

            var result = _announcementService.GetPagedList(paginationParams);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// ID'ye göre duyuru getirir
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetById(int id)
        {
            var result = _announcementService.GetById(id);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        /// <summary>
        /// Aktif duyuruları getirir (herkes için)
        /// </summary>
        [HttpGet("active")]
        public IActionResult GetActive()
        {
            var result = _announcementService.GetActiveAnnouncements();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Hedef kitleye göre aktif duyuruları getirir
        /// </summary>
        /// <param name="role">Hedef kitle (öğrenci, öğretmen, all)</param>
        [HttpGet("by-target/{role}")]
        [Authorize]
        public IActionResult GetByTargetAudience(string role)
        {
            var result = _announcementService.GetByTargetAudience(role);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Popup olarak gösterilecek duyuruları getirir
        /// </summary>
        /// <param name="role">Kullanıcı rolü</param>
        [HttpGet("popup/{role}")]
        [Authorize]
        public IActionResult GetPopup(string role)
        {
            var result = _announcementService.GetPopupAnnouncements(role);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Yeni duyuru oluşturur (Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Add([FromBody] Announcement announcement)
        {
            var result = _announcementService.Add(announcement);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        /// <summary>
        /// Duyuru günceller (Admin)
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public IActionResult Update([FromBody] Announcement announcement)
        {
            var result = _announcementService.Update(announcement);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Duyuruyu siler (Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var announcement = _announcementService.GetById(id);
            if (!announcement.Success)
                return NotFound(announcement);

            var result = _announcementService.Delete(announcement.Data);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Duyuruyu okundu olarak işaretler
        /// </summary>
        [HttpPost("mark-read")]
        [Authorize]
        public IActionResult MarkAsRead([FromQuery] int announcementId, [FromQuery] int userId)
        {
            var result = _announcementService.MarkAsRead(announcementId, userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Kullanıcı duyuruyu okudu mu kontrol eder
        /// </summary>
        [HttpGet("has-read")]
        [Authorize]
        public IActionResult HasUserRead([FromQuery] int announcementId, [FromQuery] int userId)
        {
            var result = _announcementService.HasUserRead(announcementId, userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
