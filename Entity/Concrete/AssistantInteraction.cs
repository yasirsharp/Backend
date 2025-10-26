using Core.Entities;
using System;

namespace Entity.Concrete
{
    /// <summary>
    /// YasirSharp AI - Kullanıcı etkileşim kayıtları
    /// Her kullanıcı sorusu ve bot cevabı loglanır
    /// </summary>
    public class AssistantInteraction : IEntity
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Kullanıcı ID (User tablosuna referans)
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Kullanıcının sorduğu soru
        /// </summary>
        public string Question { get; set; } = string.Empty;
        
        /// <summary>
        /// Bot'un verdiği cevap
        /// </summary>
        public string Answer { get; set; } = string.Empty;
        
        /// <summary>
        /// Sorunun sorulduğu sayfa (dashboard, calendar, bolumler, vb.)
        /// </summary>
        public string PageContext { get; set; } = string.Empty;
        
        /// <summary>
        /// Kullanılan özellik (opsiyonel)
        /// Örn: "add-exam", "view-announcements", vb.
        /// </summary>
        public string? FeatureUsed { get; set; }
        
        /// <summary>
        /// Dil (tr, en)
        /// </summary>
        public string Language { get; set; } = "tr";
        
        /// <summary>
        /// Etkileşim zamanı
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Kullanıcı geri bildirimi (thumbs up/down)
        /// null: Henüz geri bildirim verilmedi
        /// true: Yardımcı oldu (👍)
        /// false: Yardımcı olmadı (👎)
        /// </summary>
        public bool? IsHelpful { get; set; }
        
        /// <summary>
        /// Hata bildirim metni
        /// Kullanıcı "Hata Bildir" butonuna tıkladıysa açıklama
        /// </summary>
        public string? ErrorReport { get; set; }
        
        /// <summary>
        /// Feedback zamanı
        /// </summary>
        public DateTime? FeedbackTimestamp { get; set; }
        
        // IEntity properties
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Status { get; set; }
    }
}
