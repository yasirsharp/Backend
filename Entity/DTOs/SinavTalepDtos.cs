using System;

namespace Entity.DTOs
{
    public class SinavTalepCreateDto
    {
        public int SinavDetayId { get; set; }
        public string? Neden { get; set; }
        public DateTime? OnerilenTarih { get; set; }
        public string? OnerilenBaslangic { get; set; }
        public string? OnerilenBitis { get; set; }
    }

    public class SinavTalepYanitDto
    {
        /// <summary>kabul veya red</summary>
        public string Durum { get; set; } = "";
        public string? Not { get; set; }
    }

    public class SinavTalepDto
    {
        public int Id { get; set; }
        public int SinavDetayId { get; set; }
        public bool SinavVar { get; set; }
        /// <summary>Sınavın şimdiki tarihi (takvimde o haftayı açmak için); sınav silindiyse boş</summary>
        public DateTime? SinavTarihi { get; set; }
        public int BolumId { get; set; }
        public string BolumAd { get; set; } = "";
        public string SinavOzeti { get; set; } = "";
        public string TalepEdenAd { get; set; } = "";
        public string Gorev { get; set; } = "";
        public string Neden { get; set; } = "";
        public DateTime? OnerilenTarih { get; set; }
        public TimeOnly? OnerilenBaslangic { get; set; }
        public TimeOnly? OnerilenBitis { get; set; }
        public string Durum { get; set; } = "";
        public string? YanitNotu { get; set; }
        public string? YanitlayanAd { get; set; }
        public DateTime? YanitTarihi { get; set; }
        public DateTime CreatedDate { get; set; }
        /// <summary>İsteği yapan kişi bu talebi kendisi mi açtı</summary>
        public bool Benim { get; set; }
        /// <summary>İsteği yapan kişi bu talebi yanıtlayabilir mi (yönetici veya o bölümün görevlisi)</summary>
        public bool Yanitlayabilir { get; set; }
    }
}
