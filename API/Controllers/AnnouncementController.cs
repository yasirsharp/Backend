using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Abstract;
using Business.Constants;
using Core.Extensions;
using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;
using Entity.DTOs;
using System.Text.RegularExpressions;

namespace API.Controllers
{
    /// <summary>
    /// Duyurular.
    /// - Yönetici her duyuruyu yönetir.
    /// - Bölüm görevlisi sadece kendi bölüm(ler)ine duyuru yayınlar ve yalnızca bu duyuruları düzenler/siler.
    /// - Her kullanıcı, rollerinden hesaplanan hedef kitlelere ve bölümlerine yönelik duyuruları görür.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AnnouncementController : ControllerBase
    {
        private static readonly Regex BolumClaim = new(@"^(\d+\.)?BolumId$", RegexOptions.Compiled);

        private readonly IAnnouncementService _announcementService;

        public AnnouncementController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        /// <summary>Kullanıcının tüm rollerinden hedef kitleleri.</summary>
        private List<string> MyAudiences()
        {
            var audiences = new List<string> { AnnouncementAudiences.All };
            if (User.IsInRole(Roles.Ogrenci)) audiences.Add(AnnouncementAudiences.Ogrenci);
            if (User.IsInAnyRole(Roles.Personel)) audiences.Add(AnnouncementAudiences.Personel);
            if (User.IsInAnyRole(Roles.Yonetici)) audiences.Add(AnnouncementAudiences.Yonetici);
            return audiences;
        }

        /// <summary>Token'daki bölümler: öğrenci için "BolumId", bölüm görevlisi için "1.BolumId", "2.BolumId"...</summary>
        private List<int> MyBolumIds() =>
            User.Claims
                .Where(c => BolumClaim.IsMatch(c.Type))
                .Select(c => int.TryParse(c.Value, out var id) ? id : 0)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

        private bool IsYonetici => User.IsInAnyRole(Roles.Yonetici);

        /// <summary>Yönetici her duyuruyu; bölüm görevlisi kendi bölümüne yönelik veya kendi oluşturduğu duyuruyu yönetir.</summary>
        private bool CanManage(Announcement announcement) =>
            IsYonetici ||
            (announcement.TargetBolumId.HasValue && MyBolumIds().Contains(announcement.TargetBolumId.Value));

        /// <summary>Bölüm görevlisi sadece kendi bölümlerinden birini hedefleyebilir.</summary>
        private string? CheckTarget(int? targetBolumId)
        {
            if (IsYonetici)
                return null;
            if (targetBolumId == null)
                return "Bölüm görevlisi tüm bölümlere duyuru gönderemez; kendi bölümünüzü seçin.";
            if (!MyBolumIds().Contains(targetBolumId.Value))
                return "Sadece kendi bölümünüze duyuru gönderebilirsiniz.";
            return null;
        }

        /// <summary>Tüm duyurular (yönetici).</summary>
        [HttpGet]
        [Authorize(Roles = Roles.Yonetici)]
        public IActionResult GetAll()
        {
            var result = _announcementService.GetList();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Yönetim ekranı listesi: yönetici için tümü, bölüm görevlisi için kendi bölümlerine yönelik duyurular.
        /// </summary>
        [HttpGet("manage")]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult GetManageable()
        {
            var result = _announcementService.GetList();
            if (!result.Success)
                return BadRequest(result);

            var list = result.Data.Where(CanManage).ToList();
            return Ok(new SuccessDataResult<List<Announcement>>(list, $"{list.Count} duyuru bulundu."));
        }

        /// <summary>Sayfalanmış duyuru listesi (yönetici).</summary>
        [HttpGet("paged")]
        [Authorize(Roles = Roles.Yonetici)]
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

        /// <summary>Kullanıcının görebileceği yayındaki duyurular (rol ve bölüm kontrolü ile).</summary>
        [HttpGet("my-announcements")]
        public IActionResult GetMyAnnouncements()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = _announcementService.GetForUser(MyAudiences(), MyBolumIds(), userId.Value, popupOnly: false);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>Girişte açılır pencerede gösterilecek, henüz okunmamış duyurular.</summary>
        [HttpGet("my-popup")]
        public IActionResult GetMyPopup()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = _announcementService.GetForUser(MyAudiences(), MyBolumIds(), userId.Value, popupOnly: true);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>Yayındaki tüm duyurular (yönetici; hedef kitle ayrımı yapılmaz).</summary>
        [HttpGet("active")]
        [Authorize(Roles = Roles.Yonetici)]
        public IActionResult GetActive()
        {
            var result = _announcementService.GetActiveAnnouncements();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult GetById(int id)
        {
            var result = _announcementService.GetById(id);
            if (!result.Success)
                return NotFound(result);
            if (!CanManage(result.Data))
                return Forbid();
            return Ok(result);
        }

        /// <summary>Yeni duyuru. Bölüm görevlisi sadece kendi bölümüne gönderebilir.</summary>
        [HttpPost]
        [HttpPost("create")]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult Create([FromBody] AnnouncementCreateDto dto)
        {
            var targetError = CheckTarget(dto.TargetBolumId);
            if (targetError != null)
                return BadRequest(new ErrorResult(targetError));

            var announcement = new Announcement
            {
                Title = dto.Title?.Trim() ?? "",
                Content = dto.Content?.Trim() ?? "",
                Type = dto.Type,
                Priority = dto.Priority,
                TargetAudience = dto.TargetAudience,
                TargetBolumId = dto.TargetBolumId,
                PublishDate = dto.PublishDate,
                ExpiryDate = dto.ExpiryDate,
                ShowAsPopup = dto.ShowAsPopup,
                IsActive = dto.IsActive,
                CreatedBy = User.GetUserId() ?? 0,
                CreatedDate = DateTime.Now
            };

            var validation = _announcementService.Validate(announcement);
            if (!validation.Success)
                return BadRequest(validation);

            var result = _announcementService.Add(announcement);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpPut]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult Update([FromBody] AnnouncementUpdateDto dto)
        {
            var existingResult = _announcementService.GetById(dto.Id);
            if (!existingResult.Success)
                return NotFound(existingResult);

            var announcement = existingResult.Data;
            if (!CanManage(announcement))
                return Forbid();

            var targetError = CheckTarget(dto.TargetBolumId);
            if (targetError != null)
                return BadRequest(new ErrorResult(targetError));

            announcement.Title = dto.Title?.Trim() ?? "";
            announcement.Content = dto.Content?.Trim() ?? "";
            announcement.Type = dto.Type;
            announcement.Priority = dto.Priority;
            announcement.TargetAudience = dto.TargetAudience;
            announcement.TargetBolumId = dto.TargetBolumId;
            announcement.TargetBolum = null;
            announcement.PublishDate = dto.PublishDate;
            announcement.ExpiryDate = dto.ExpiryDate;
            announcement.ShowAsPopup = dto.ShowAsPopup;
            announcement.IsActive = dto.IsActive;
            announcement.UpdatedDate = DateTime.Now;

            var validation = _announcementService.Validate(announcement);
            if (!validation.Success)
                return BadRequest(validation);

            var result = _announcementService.Update(announcement);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult Delete(int id)
        {
            var announcement = _announcementService.GetById(id);
            if (!announcement.Success)
                return NotFound(announcement);
            if (!CanManage(announcement.Data))
                return Forbid();

            announcement.Data.TargetBolum = null;
            var result = _announcementService.Delete(announcement.Data);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>Duyuruyu giriş yapan kullanıcı için okundu işaretler (açılır pencere bir daha gösterilmez).</summary>
        [HttpPost("{id}/read")]
        public IActionResult MarkMyRead(int id)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = _announcementService.MarkAsRead(id, userId.Value);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>Eski istemciler için: kullanıcı sadece kendi adına işaretleyebilir.</summary>
        [HttpPost("mark-read")]
        public IActionResult MarkAsRead([FromQuery] int announcementId, [FromQuery] int userId)
        {
            if (User.GetUserId() != userId)
                return Forbid();

            var result = _announcementService.MarkAsRead(announcementId, userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("has-read")]
        public IActionResult HasUserRead([FromQuery] int announcementId, [FromQuery] int userId)
        {
            if (User.GetUserId() != userId && !User.IsInAnyRole(Roles.Yonetici))
                return Forbid();

            var result = _announcementService.HasUserRead(announcementId, userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
