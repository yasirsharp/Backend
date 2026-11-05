using Core.Entities;
using System;

namespace Entity.Concrete
{
    /// <summary>
    /// Akademik personelin dersini verdiği ya da gözetmen olduğu bir sınav için değişiklik isteği.
    /// Bölüm görevlisi veya yönetici kabul eder (sınavı kendisi düzenler) ya da gerekçeyle reddeder.
    /// </summary>
    public class SinavDegisiklikTalebi : IEntity
    {
        public int Id { get; set; }
        public int SinavDetayId { get; set; }
        /// <summary>Talep anındaki bölüm (yetki ve listeleme için; sınav silinse de talep görünür)</summary>
        public int BolumId { get; set; }
        /// <summary>Talep anındaki sınav özeti (ör. "BLM201 Veri Yapıları · 12.11.2026 10:00-12:00")</summary>
        public string SinavOzeti { get; set; } = "";
        public int TalepEdenUserId { get; set; }
        public string TalepEdenAd { get; set; } = "";
        /// <summary>ogretim-elemani veya gozetmen</summary>
        public string Gorev { get; set; } = "";
        public string Neden { get; set; } = "";
        public DateTime? OnerilenTarih { get; set; }
        public TimeOnly? OnerilenBaslangic { get; set; }
        public TimeOnly? OnerilenBitis { get; set; }
        /// <summary>bekliyor, kabul, red</summary>
        public string Durum { get; set; } = SinavTalepDurumlari.Bekliyor;
        public string? YanitNotu { get; set; }
        public int? YanitlayanUserId { get; set; }
        public string? YanitlayanAd { get; set; }
        public DateTime? YanitTarihi { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Status { get; set; } = true;
    }

    public static class SinavTalepDurumlari
    {
        public const string Bekliyor = "bekliyor";
        public const string Kabul = "kabul";
        public const string Red = "red";
    }
}
