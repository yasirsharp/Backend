namespace Entity.DTOs
{
    /// <summary>
    /// Duyuru oluşturma için DTO
    /// </summary>
    public class AnnouncementCreateDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Type { get; set; } // general, urgent, maintenance, event
        public int Priority { get; set; } // 0=Normal, 1=Önemli, 2=Acil
        public string TargetAudience { get; set; } // all, admin, gorevli.personel, personel, ogrenci
        public int? TargetBolumId { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool ShowAsPopup { get; set; }
        public bool IsActive { get; set; } = true;
        public int CreatedBy { get; set; }
    }
}
