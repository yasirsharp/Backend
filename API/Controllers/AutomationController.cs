using System.Security.Cryptography;
using System.Text;
using Business.Abstract;
using Core.Utilities.Results;
using Entity.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
    /// <summary>
    /// Gece çalışan otomatik inceleme için dar yetkili uçlar: yalnızca hata bildirimlerini okur ve durum/not yazar.
    /// Kimlik: X-Automation-Key başlığı = BugReports:AutomationKey (en az 32 karakter). Ayar yoksa uçlar yok sayılır (404).
    /// </summary>
    [Route("api/automation/bug-reports")]
    [ApiController]
    [AllowAnonymous]
    [EnableRateLimiting("automation")]
    [ServiceFilter(typeof(AutomationKeyFilter))]
    public class AutomationController : ControllerBase
    {
        private readonly IBugReportService _bugReportService;

        public AutomationController(IBugReportService bugReportService)
        {
            _bugReportService = bugReportService;
        }

        [HttpGet]
        public IActionResult GetList([FromQuery] string? state = null)
        {
            var result = _bugReportService.GetList(state);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:int}/screenshot")]
        public IActionResult GetScreenshot(int id)
        {
            var shot = _bugReportService.GetScreenshot(id);
            return shot == null ? NotFound() : File(shot.Value.Data, shot.Value.ContentType);
        }

        [HttpPut("{id:int}/state")]
        public IActionResult UpdateState(int id, [FromBody] BugReportStateDto dto)
        {
            var result = _bugReportService.UpdateState(id, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }

    public class AutomationKeyFilter : IActionFilter
    {
        public const int MinKeyLength = 32;
        private readonly IConfiguration _configuration;

        public AutomationKeyFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static bool KeyMatches(string? configured, string? provided)
        {
            if (string.IsNullOrEmpty(configured) || configured.Length < MinKeyLength || string.IsNullOrEmpty(provided))
                return false;
            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(configured), Encoding.UTF8.GetBytes(provided));
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var configured = _configuration["BugReports:AutomationKey"];
            if (string.IsNullOrEmpty(configured) || configured.Length < MinKeyLength)
            {
                context.Result = new NotFoundResult();
                return;
            }
            if (!KeyMatches(configured, context.HttpContext.Request.Headers["X-Automation-Key"].ToString()))
                context.Result = new UnauthorizedObjectResult(new ErrorResult("Geçersiz otomasyon anahtarı."));
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
