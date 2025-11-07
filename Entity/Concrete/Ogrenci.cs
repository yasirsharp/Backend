using Core.Entities;

namespace Entity.Concrete
{
    /// <summary>
    /// Öğrenci Entity
    /// Öğrenci bilgilerini tutar
    /// User tablosu ile 1-to-1 relationship
    /// </summary>
    public class Ogrenci : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string OgrenciNo { get; set; } // Unique - örn: "2021123456"
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public int BolumId { get; set; }
        public int? Sinif { get; set; } // 1, 2, 3, 4 (Opsiyonel)
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Status { get; set; } // true: Aktif, false: Pasif
    }
}
