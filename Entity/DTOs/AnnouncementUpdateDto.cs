namespace Entity.DTOs
{
    /// <summary>
    /// Duyuru güncelleme için DTO
    /// </summary>
    public class AnnouncementUpdateDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Type { get; set; } // general, urgent, maintenance, event
        public int Priority { get; set; } // 0=Normal, 1=Önemli, 2=Acil
        public string TargetAudience { get; set; } // all, admin, gorevli.personel, personel, ogrenci
        public int? TargetBolumId { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool ShowAsPopup { get; set; }
        public bool IsActive { get; set; }
    }
}
