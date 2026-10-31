using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Abstract;
using Business.Constants;
using Core.Extensions;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Tüm bildirimleri getirir (Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = Roles.Yonetici)]
        public IActionResult GetAll()
        {
            var result = _notificationService.GetList();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Sayfalanmış bildirim listesi
        /// </summary>
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

            var result = _notificationService.GetPagedList(paginationParams);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// ID'ye göre bildirim getirir
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var result = _notificationService.GetById(id);
            if (!result.Success)
                return NotFound(result);
            if (!CanAccessUser(result.Data.UserId))
                return Forbid();
            return Ok(result);
        }

        /// <summary>
        /// Kullanıcıya ait tüm bildirimleri getirir
        /// </summary>
        [HttpGet("user/{userId}")]
        public IActionResult GetByUserId(int userId)
        {
            if (!CanAccessUser(userId))
                return Forbid();

            var result = _notificationService.GetByUserId(userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Kullanıcıya ait okunmamış bildirimleri getirir
        /// </summary>
        [HttpGet("user/{userId}/unread")]
        public IActionResult GetUnreadByUserId(int userId)
        {
            if (!CanAccessUser(userId))
                return Forbid();

            var result = _notificationService.GetUnreadByUserId(userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Yeni bildirim oluşturur
        /// </summary>
        [HttpPost]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult Add([FromBody] Notification notification)
        {
            var result = _notificationService.Add(notification);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        /// <summary>
        /// Bildirimi okundu olarak işaretler
        /// </summary>
        [HttpPut("mark-read/{id}")]
        public IActionResult MarkAsRead(int id)
        {
            var notification = _notificationService.GetById(id);
            if (!notification.Success)
                return NotFound(notification);
            if (!CanAccessUser(notification.Data.UserId))
                return Forbid();

            var result = _notificationService.MarkAsRead(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Kullanıcının tüm bildirimlerini okundu olarak işaretler
        /// </summary>
        [HttpPut("mark-all-read/{userId}")]
        public IActionResult MarkAllAsRead(int userId)
        {
            if (!CanAccessUser(userId))
                return Forbid();

            var result = _notificationService.MarkAllAsRead(userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Bildirimi siler
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var notification = _notificationService.GetById(id);
            if (!notification.Success)
                return NotFound(notification);
            if (!CanAccessUser(notification.Data.UserId))
                return Forbid();

            var result = _notificationService.Delete(notification.Data);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Kullanıcı sadece kendi bildirimlerine erişebilir; yöneticiler herkesinkine.
        /// </summary>
        private bool CanAccessUser(int userId)
        {
            return User.GetUserId() == userId || User.IsInAnyRole(Roles.Yonetici);
        }
    }
}
