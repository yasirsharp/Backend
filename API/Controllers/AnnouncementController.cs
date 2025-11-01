using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Abstract;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;
using Entity.DTOs;
using System.Security.Claims; // 🆕 Token claims için
using System.Linq; // 🆕

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
        [Authorize(Roles = "Admin,super.admin")]
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
        [Authorize(Roles = "Admin,super.admin")]
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
        /// 🆕 Kullanıcı için geçerli duyuruları getirir (rol ve bölüm kontrolü ile)
        /// Token'dan rol ve BolumId alınır
        /// </summary>
        [HttpGet("my-announcements")]
        [Authorize]
        public IActionResult GetMyAnnouncements()
        {
            // Token'dan bilgileri al
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "ogrenci";
            var userBolumIdClaim = User.FindFirst("BolumId")?.Value;
            int? userBolumId = !string.IsNullOrEmpty(userBolumIdClaim) 
                ? int.Parse(userBolumIdClaim) 
                : null;

            var result = _announcementService.GetByUserId(userRole, userBolumId);
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
        /// 🆕 Kullanıcı için popup duyuruları getirir (bölüm kontrolü ile)
        /// Token'dan rol ve BolumId alınır
        /// </summary>
        [HttpGet("my-popup")]
        [Authorize]
        public IActionResult GetMyPopup()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "ogrenci";
            var userBolumIdClaim = User.FindFirst("BolumId")?.Value;
            int? userBolumId = !string.IsNullOrEmpty(userBolumIdClaim) 
                ? int.Parse(userBolumIdClaim) 
                : null;

            var result = _announcementService.GetPopupAnnouncementsByUser(userRole, userBolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Yeni duyuru oluşturur (Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "super.admin")]
        public IActionResult Add([FromBody] AnnouncementCreateDto dto)
        {
            var announcement = new Announcement
            {
                Title = dto.Title,
                Content = dto.Content,
                Type = dto.Type,
                Priority = dto.Priority,
                TargetAudience = dto.TargetAudience,
                TargetBolumId = dto.TargetBolumId,
                PublishDate = dto.PublishDate,
                ExpiryDate = dto.ExpiryDate,
                ShowAsPopup = dto.ShowAsPopup,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.Now,
                IsActive = dto.IsActive
            };

            var result = _announcementService.Add(announcement);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }
        
        /// <summary>
        /// 🆕 Yeni duyuru oluşturur (Yetki kontrolü ile)
        /// Admin: Herkese gönderebilir
        /// Görevli Personel: Sadece kendi bölümüne gönderebilir
        /// </summary>
        [HttpPost("create")]
        [Authorize(Roles = "Admin,super.admin,gorevli.personel")]
        public IActionResult CreateWithPermission([FromBody] AnnouncementCreateDto dto)
        {
            // Token'dan bilgileri al
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "ogrenci";
            var userBolumIdClaim = User.FindFirst("BolumId")?.Value;
            int? userBolumId = !string.IsNullOrEmpty(userBolumIdClaim) 
                ? int.Parse(userBolumIdClaim) 
                : null;

            // DTO'dan entity oluştur
            var announcement = new Announcement
            {
                Title = dto.Title,
                Content = dto.Content,
                Type = dto.Type,
                Priority = dto.Priority,
                TargetAudience = dto.TargetAudience,
                TargetBolumId = dto.TargetBolumId,
                PublishDate = dto.PublishDate,
                ExpiryDate = dto.ExpiryDate,
                ShowAsPopup = dto.ShowAsPopup,
                IsActive = dto.IsActive,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.Now
            };

            var result = _announcementService.AddWithPermission(announcement, userRole, userBolumId);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        /// <summary>
        /// Duyuru günceller (Admin)
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "Admin,super.admin")]
        public IActionResult Update([FromBody] AnnouncementUpdateDto dto)
        {
            // Önce mevcut duyuruyu getir
            var existingResult = _announcementService.GetById(dto.Id);
            if (!existingResult.Success)
                return NotFound(existingResult);

            var announcement = existingResult.Data;
            
            // DTO'dan gelen değerleri güncelle
            announcement.Title = dto.Title;
            announcement.Content = dto.Content;
            announcement.Type = dto.Type;
            announcement.Priority = dto.Priority;
            announcement.TargetAudience = dto.TargetAudience;
            announcement.TargetBolumId = dto.TargetBolumId;
            announcement.PublishDate = dto.PublishDate;
            announcement.ExpiryDate = dto.ExpiryDate;
            announcement.ShowAsPopup = dto.ShowAsPopup;
            announcement.IsActive = dto.IsActive;

            var result = _announcementService.Update(announcement);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Duyuruyu siler (Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,super.admin")]
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
