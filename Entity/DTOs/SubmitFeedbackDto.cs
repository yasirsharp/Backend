namespace Entity.DTOs
{
    /// <summary>
    /// Kullanıcı geri bildirimi (thumbs up/down veya hata bildirimi)
    /// </summary>
    public class SubmitFeedbackDto
    {
        /// <summary>
        /// Etkileşim ID (AssistantInteraction tablosundan)
        /// </summary>
        public int InteractionId { get; set; }
        
        /// <summary>
        /// Yardımcı oldu mu? (null: henüz oy verilmedi, true: 👍, false: 👎)
        /// </summary>
        public bool? IsHelpful { get; set; }
        
        /// <summary>
        /// Hata bildirim metni (opsiyonel)
        /// "Hata Bildir" butonuna tıklandıysa doldurulur
        /// </summary>
        public string? ErrorReport { get; set; }
    }
}
