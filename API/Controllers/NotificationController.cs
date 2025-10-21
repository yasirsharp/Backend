using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Abstract;
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
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        /// <summary>
        /// Kullanıcıya ait tüm bildirimleri getirir
        /// </summary>
        [HttpGet("user/{userId}")]
        public IActionResult GetByUserId(int userId)
        {
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
            var result = _notificationService.GetUnreadByUserId(userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Yeni bildirim oluşturur
        /// </summary>
        [HttpPost]
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

            var result = _notificationService.Delete(notification.Data);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
