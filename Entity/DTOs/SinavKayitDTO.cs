using Core.Entities;

namespace Entity.DTOs
{
    public class SinavKayitDTO:IDto
    {
        public int DersBolumAkademikPersonelId { get; set; }  // Fixed typo: added 's'
        public DateTime SinavTarihi { get; set; }
        public string SinavBaslangicSaati { get; set; }
        public string SinavBitisSaati { get; set; }
        public List<DerslikGozetmenDTO> Derslikler { get; set; }
    }
}
