using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Abstract;
using Entity.Concrete;

namespace API.Controllers
{
    /// <summary>
    /// YasirSharp AI - User Preference Controller
    /// </summary>
    [Route("api/user-assistant-preference")] // Frontend route'u ile eşleştirdik
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
        /// GET /api/user-assistant-preference/{userId}
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
        /// Tercihleri güncelle (Partial Update)
        /// PUT /api/user-assistant-preference/{userId}
        /// Body: { "isEnabled": true, "preferredLanguage": "tr" } (partial)
        /// </summary>
        [HttpPut("{userId}")]
        public IActionResult UpdatePreference(int userId, [FromBody] UpdatePreferenceDto dto)
        {
            // Önce mevcut preference'ı al
            var existingResult = _preferenceService.GetPreference(userId);
            
            if (!existingResult.Success || existingResult.Data == null)
            {
                return NotFound(new { success = false, message = "Kullanıcı tercihleri bulunamadı." });
            }

            var preference = existingResult.Data;

            // Partial update - sadece gönderilen alanları güncelle
            if (dto.IsEnabled.HasValue)
                preference.IsEnabled = dto.IsEnabled.Value;
            
            if (!string.IsNullOrEmpty(dto.PreferredLanguage))
                preference.PreferredLanguage = dto.PreferredLanguage;

            // Güncelle
            var result = _preferenceService.UpdatePreference(preference);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Onboarding'i tamamla
        /// PUT /api/user-assistant-preference/{userId}/complete-onboarding
        /// </summary>
        [HttpPut("{userId}/complete-onboarding")]
        public IActionResult CompleteOnboarding(int userId)
        {
            var result = _preferenceService.CompleteOnboarding(userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Bot'u aç/kapat
        /// PUT /api/user-assistant-preference/{userId}/toggle
        /// Body: { "isEnabled": true }
        /// </summary>
        [HttpPut("{userId}/toggle")]
        public IActionResult ToggleBot(int userId, [FromBody] ToggleBotDto dto)
        {
            var result = _preferenceService.ToggleBot(userId, dto.IsEnabled);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }

    /// <summary>
    /// Toggle bot DTO
    /// </summary>
    public class ToggleBotDto
    {
        public bool IsEnabled { get; set; }
    }

    /// <summary>
    /// Update preference DTO (Partial Update için)
    /// </summary>
    public class UpdatePreferenceDto
    {
        public bool? IsEnabled { get; set; }
        public string? PreferredLanguage { get; set; }
    }
}
