using System.Collections.Generic;

namespace Entity.DTOs
{
    /// <summary>
    /// YasirSharp AI - Bot yanıt DTO
    /// </summary>
    public class BotResponseDto
    {
        public int InteractionId { get; set; } // Feedback için gerekli
        public string Answer { get; set; } = string.Empty;
        public List<QuickActionDto> SuggestedActions { get; set; } = new();
        public string PageGuideReference { get; set; } = string.Empty;
        public string DetectedIntent { get; set; } = string.Empty;
    }
}
