using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Abstract;
using Entity.DTOs;

namespace API.Controllers
{
    /// <summary>
    /// YasirSharp AI - Assistant Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Tüm endpoint'ler authentication gerektirir
    public class AssistantController : ControllerBase
    {
        private readonly IAssistantService _assistantService;

        public AssistantController(IAssistantService assistantService)
        {
            _assistantService = assistantService;
        }

        /// <summary>
        /// ⭐ Ana endpoint: Kullanıcı sorusuna akıllı cevap üret
        /// POST /api/Assistant/ask
        /// </summary>
        [HttpPost("ask")]
        public IActionResult AskQuestion([FromBody] AskQuestionDto dto)
        {
            var result = _assistantService.AskQuestion(dto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Sayfa rehberini getir
        /// GET /api/Assistant/page-guide/{pageName}?language=tr
        /// </summary>
        [HttpGet("page-guide/{pageName}")]
        public IActionResult GetPageGuide(string pageName, [FromQuery] string language = "tr")
        {
            var result = _assistantService.GetPageGuide(pageName, language);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        /// <summary>
        /// Hızlı aksiyonları getir (rol bazlı)
        /// GET /api/Assistant/quick-actions?role=admin&language=tr
        /// </summary>
        [HttpGet("quick-actions")]
        public IActionResult GetQuickActions([FromQuery] string role, [FromQuery] string language = "tr")
        {
            var result = _assistantService.GetQuickActions(role, language);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Kullanıcı-bot etkileşimini logla
        /// POST /api/Assistant/log-interaction
        /// </summary>
        [HttpPost("log-interaction")]
        public IActionResult LogInteraction([FromBody] LogInteractionDto dto)
        {
            var result = _assistantService.LogInteraction(dto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Kullanıcının etkileşim geçmişini getir
        /// GET /api/Assistant/history/{userId}?limit=10
        /// </summary>
        [HttpGet("history/{userId}")]
        public IActionResult GetHistory(int userId, [FromQuery] int limit = 10)
        {
            var result = _assistantService.GetUserHistory(userId, limit);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Analytics istatistikleri (Admin için)
        /// GET /api/Assistant/analytics
        /// </summary>
        [HttpGet("analytics")]
        [Authorize(Roles = "Admin,super.admin")] // Sadece admin erişebilir
        public IActionResult GetAnalytics()
        {
            var result = _assistantService.GetAnalytics();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// 👍👎 Kullanıcı geri bildirimi kaydet (Thumbs up/down veya hata bildirimi)
        /// POST /api/Assistant/feedback
        /// </summary>
        [HttpPost("feedback")]
        public IActionResult SubmitFeedback([FromBody] SubmitFeedbackDto dto)
        {
            var result = _assistantService.SubmitFeedback(dto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// 🐛 Hata bildirimleri listesini getir (Admin için)
        /// GET /api/Assistant/error-reports?skip=0&take=50
        /// </summary>
        [HttpGet("error-reports")]
        [Authorize(Roles = "Admin,super.admin")]
        public IActionResult GetErrorReports([FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            var result = _assistantService.GetErrorReports(skip, take);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
