using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Abstract;
using Entity.Concrete;

namespace API.Controllers
{
    /// <summary>
    /// YasirSharp AI - User Preference Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Tüm endpoint'ler authentication gerektirir
    public class UserAssistantPreferenceController : ControllerBase
    {
        private readonly IUserAssistantPreferenceService _preferenceService;

        public UserAssistantPreferenceController(IUserAssistantPreferenceService preferenceService)
        {
            _preferenceService = preferenceService;
        }

        /// <summary>
        /// Kullanıcının tercihlerini getir
        /// GET /api/UserAssistantPreference/{userId}
        /// </summary>
        [HttpGet("{userId}")]
        public IActionResult GetPreference(int userId)
        {
            var result = _preferenceService.GetPreference(userId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        /// <summary>
        /// Tercihleri güncelle
        /// PUT /api/UserAssistantPreference
        /// </summary>
        [HttpPut]
        public IActionResult UpdatePreference([FromBody] UserAssistantPreference preference)
        {
            var result = _preferenceService.UpdatePreference(preference);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Onboarding'i tamamla
        /// POST /api/UserAssistantPreference/complete-onboarding/{userId}
        /// </summary>
        [HttpPost("complete-onboarding/{userId}")]
        public IActionResult CompleteOnboarding(int userId)
        {
            var result = _preferenceService.CompleteOnboarding(userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Bot'u aç/kapat
        /// POST /api/UserAssistantPreference/toggle/{userId}
        /// Body: { "isEnabled": true }
        /// </summary>
        [HttpPost("toggle/{userId}")]
        public IActionResult ToggleBot(int userId, [FromBody] ToggleBotDto dto)
        {
            var result = _preferenceService.ToggleBot(userId, dto.IsEnabled);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }

    /// <summary>
    /// Toggle bot DTO (Body için)
    /// </summary>
    public class ToggleBotDto
    {
        public bool IsEnabled { get; set; }
    }
}
