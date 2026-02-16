using System;

namespace Entity.DTOs
{
    /// <summary>
    /// Müsaitlik kaydı yanıt DTO (API'den dönen veri)
    /// Entity'nin güvenli şekilde dışarıya sunulması
    /// </summary>
    public class MusaitlikResponseDto
    {
        public int Id { get; set; }
        public int AkademikPersonelId { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }
        public TimeSpan? BaslangicSaati { get; set; }
        public TimeSpan? BitisSaati { get; set; }
        public int TekrarTipi { get; set; }
        public int? TekrarGunu { get; set; }
        public bool IsMusait { get; set; }
        public string? Neden { get; set; }
        public string? Aciklama { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
