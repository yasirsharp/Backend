using Core.Entities;

namespace Entity.DTOs
{
    /// <summary>Sınav bildirimleri için sınavın özeti ve bildirim gidecek kişiler.</summary>
    public class SinavBildirimBilgisiDTO : IDto
    {
        public int SinavId { get; set; }
        public string DersKod { get; set; } = "";
        public string DersAd { get; set; } = "";
        public DateTime SinavTarihi { get; set; }
        public TimeOnly BaslangicSaati { get; set; }
        public TimeOnly BitisSaati { get; set; }
        public List<string> Derslikler { get; set; } = new();
        public List<SinavGorevliDTO> Gorevliler { get; set; } = new();
    }

    public class SinavGorevliDTO : IDto
    {
        public int UserId { get; set; }
        /// <summary>"ogretim" (dersin öğretim elemanı) veya "gozetmen"</summary>
        public string Gorev { get; set; } = "";
        /// <summary>Gözetmenlik için derslik adı</summary>
        public string? Derslik { get; set; }
    }
}
