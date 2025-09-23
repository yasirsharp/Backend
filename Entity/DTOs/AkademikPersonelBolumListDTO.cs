using Core.Entities;
using Entity.Concrete;

namespace Entity.DTOs
{
    public class AkademikPersonelBolumListDTO : IDto
    {
        public int AkademikPersonelId { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string TamAd => $"{Ad} {Soyad}";
        public string Unvan { get; set; }
        public List<Bolum> AtanmisBolumler { get; set; }
    }
}
