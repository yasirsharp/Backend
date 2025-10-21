using System;

namespace Entity.Concrete
{
    /// <summary>
    /// Duyuru okuma durumu takibi için junction table
    /// Hangi kullanıcı hangi duyuruyu okudu?
    /// </summary>
    public class AnnouncementReadStatus
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Duyuru ID'si
        /// </summary>
        public int AnnouncementId { get; set; }
        
        /// <summary>
        /// Kullanıcı ID'si
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Duyurunun okunma tarihi
        /// </summary>
        public DateTime ReadDate { get; set; }
        
        // NOT: Bu entity IEntity'den türemez çünkü basit bir junction table
        // CreatedDate yerine ReadDate kullanıyoruz
    }
}
