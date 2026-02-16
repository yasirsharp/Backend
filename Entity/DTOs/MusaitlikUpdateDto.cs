using System;
using System.ComponentModel.DataAnnotations;
using Entity.Concrete;

namespace Entity.DTOs
{
    /// <summary>
    /// Müsaitlik kaydı güncelleme DTO
    /// </summary>
    public class MusaitlikUpdateDto
    {
        [Required(ErrorMessage = "Kayıt ID gereklidir.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Akademik personel ID gereklidir.")]
        public int AkademikPersonelId { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi gereklidir.")]
        public DateTime BaslangicTarihi { get; set; }

        public DateTime? BitisTarihi { get; set; }

        public TimeSpan? BaslangicSaati { get; set; }
        public TimeSpan? BitisSaati { get; set; }

        [Required(ErrorMessage = "Tekrar tipi gereklidir.")]
        public TekrarTipiEnum TekrarTipi { get; set; } = TekrarTipiEnum.Tekil;

        /// <summary>
        /// Müsaitlik durumu: true = Müsait, false = Meşgul
        /// </summary>
        [Required(ErrorMessage = "Müsaitlik durumu gereklidir.")]
        public bool IsMusait { get; set; } = false;

        [MaxLength(100, ErrorMessage = "Neden en fazla 100 karakter olabilir.")]
        public string? Neden { get; set; }

        [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Aciklama { get; set; }
    }
}
