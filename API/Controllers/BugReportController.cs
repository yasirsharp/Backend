using System.Security.Claims;
using API.Services;
using Business.Abstract;
using Business.Constants;
using Business.Helpers;
using Core.Extensions;
using Core.Utilities.Results;
using Entity.Concrete;
using Entity.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
    /// <summary>
    /// Uygulama içinden gönderilen hata bildirimleri. Her giriş yapmış kullanıcı gönderebilir; sadece yönetici görür.
    /// Kullanıcı bilgisi token'dan alınır, istemcinin gönderdiğine güvenilmez.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BugReportController : ControllerBase
    {
        private readonly IBugReportService _bugReportService;
        private readonly BugReportMailer _mailer;

        public BugReportController(IBugReportService bugReportService, BugReportMailer mailer)
        {
            _bugReportService = bugReportService;
            _mailer = mailer;
        }

        public class CreateForm
        {
            public string? Description { get; set; }
            public string? PageUrl { get; set; }
            public string? AppVersion { get; set; }
            public string? Viewport { get; set; }
            public string? ClientLog { get; set; }
            public IFormFile? Screenshot { get; set; }
        }

        [HttpPost]
        [RequestSizeLimit(3_000_000)]
        [EnableRateLimiting("bug-report")]
        public async Task<IActionResult> Create([FromForm] CreateForm form)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            byte[]? screenshot = null;
            if (form.Screenshot is { Length: > 0 })
            {
                if (form.Screenshot.Length > BugReportRules.MaxScreenshotBytes)
                    return BadRequest(new ErrorResult("Ekran görüntüsü çok büyük (en fazla 1,5 MB)."));
                using var ms = new MemoryStream();
                await form.Screenshot.CopyToAsync(ms);
                screenshot = ms.ToArray();
            }

            var name = User.FindFirst(ClaimTypes.Name)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
            var report = new BugReport
            {
                UserId = userId.Value,
                UserName = string.Join(" · ", new[] { name, email }.Where(x => !string.IsNullOrWhiteSpace(x))),
                Roles = string.Join(", ", User.ClaimRoles() ?? new List<string>()),
                Description = form.Description ?? "",
                PageUrl = form.PageUrl ?? "",
                AppVersion = form.AppVersion ?? "",
                UserAgent = Request.Headers.UserAgent.ToString(),
                Viewport = form.Viewport ?? "",
                ClientLog = form.ClientLog,
                Screenshot = screenshot,
            };

            var result = _bugReportService.Add(report);
            if (!result.Success)
                return BadRequest(result);

            _mailer.SendInBackground(report, $"{Request.Scheme}://{Request.Host}/yonetim/hata-bildirimleri?id={report.Id}");
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = Roles.Yonetici)]
        public IActionResult GetList([FromQuery] string? state = null)
        {
            var result = _bugReportService.GetList(state);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Menüdeki rozet için yeni bildirim sayısı</summary>
        [HttpGet("new-count")]
        [Authorize(Roles = Roles.Yonetici)]
        public IActionResult GetNewCount() => Ok(_bugReportService.CountNew());

        [HttpGet("{id:int}")]
        [Authorize(Roles = Roles.Yonetici)]
        public IActionResult GetById(int id)
        {
            var result = _bugReportService.GetById(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("{id:int}/screenshot")]
        [Authorize(Roles = Roles.Yonetici)]
        public IActionResult GetScreenshot(int id)
        {
            var shot = _bugReportService.GetScreenshot(id);
            if (shot == null)
                return NotFound();
            Response.Headers.CacheControl = "private, no-store";
            return File(shot.Value.Data, shot.Value.ContentType);
        }

        [HttpPut("{id:int}/state")]
        [Authorize(Roles = Roles.Yonetici)]
        public IActionResult UpdateState(int id, [FromBody] BugReportStateDto dto)
        {
            var result = _bugReportService.UpdateState(id, dto);
            if (result.Success)
                return Ok(result);
            return result.Message == "Hata bildirimi bulunamadı." ? NotFound(result) : BadRequest(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Yonetici)]
        public IActionResult Delete(int id)
        {
            var result = _bugReportService.Delete(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}
