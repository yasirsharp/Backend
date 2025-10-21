using Core.Entities;
using System;

namespace Entity.Concrete
{
    /// <summary>
    /// Kullanıcıya özel bildirim entity'si
    /// </summary>
    public class Notification : IEntity
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Bildirimin gönderildiği kullanıcı ID'si
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Bildirim başlığı
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Bildirim içeriği/mesajı
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Bildirim tipi: info, success, warning, error
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// Bildirim okundu mu?
        /// </summary>
        public bool IsRead { get; set; }
        
        /// <summary>
        /// Tıklanınca gidilecek URL (opsiyonel)
        /// </summary>
        public string ActionUrl { get; set; }
        
        /// <summary>
        /// İlgili entity tipi (SinavDetay, Ders, Bolum, vb.) - Opsiyonel
        /// </summary>
        public string RelatedEntityType { get; set; }
        
        /// <summary>
        /// İlgili entity'nin ID'si - Opsiyonel
        /// </summary>
        public int? RelatedEntityId { get; set; }
        
        /// <summary>
        /// Bildirim okunma tarihi
        /// </summary>
        public DateTime? ReadDate { get; set; }

        // IEntity properties
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Status { get; set; }
    }
}
