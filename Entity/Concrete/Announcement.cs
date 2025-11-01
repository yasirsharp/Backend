using Core.Entities;
using System;

namespace Entity.Concrete
{
    /// <summary>
    /// Duyuru entity'si - Toplu, rol bazlı duyurular için
    /// </summary>
    public class Announcement : IEntity
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Duyuru başlığı
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Duyuru içeriği (HTML formatında olabilir)
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// Duyuru tipi: general, urgent, maintenance, event
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// Öncelik seviyesi: 0=Normal, 1=Önemli, 2=Acil
        /// </summary>
        public int Priority { get; set; }
        
        /// <summary>
        /// Hedef kitle: all, admin, gorevli.personel, personel, ogrenci
        /// </summary>
        public string TargetAudience { get; set; }
        
        /// <summary>
        /// 🆕 Belirli bir bölüme özel duyuru (NULL = tüm bölümler)
        /// Görevli Personel sadece kendi bölümüne duyuru gönderebilir
        /// </summary>
        public int? TargetBolumId { get; set; }
        
        /// <summary>
        /// Navigation property - Hedef Bölüm
        /// </summary>
        public virtual Bolum TargetBolum { get; set; }
        
        /// <summary>
        /// Duyuru yayınlanma tarihi
        /// </summary>
        public DateTime PublishDate { get; set; }
        
        /// <summary>
        /// Duyuru bitiş tarihi (opsiyonel)
        /// </summary>
        public DateTime? ExpiryDate { get; set; }
        
        /// <summary>
        /// Duyuru aktif mi?
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Popup olarak gösterilsin mi?
        /// </summary>
        public bool ShowAsPopup { get; set; }
        
        /// <summary>
        /// Duyuruyu oluşturan kullanıcı ID'si
        /// </summary>
        public int CreatedBy { get; set; }

        // IEntity properties
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Status { get; set; }
    }
}
