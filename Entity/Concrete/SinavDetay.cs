using Core.Entities;

namespace Entity.Concrete
{
    public class SinavDetay : IEntity
    {
        public int Id { get; set; }
        public int DersBolumAkademikPersonelId { get; set; }
        public DateTime SinavTarihi { get; set; }
        public TimeOnly SinavBaslangicSaati { get; set; }
        public TimeOnly SinavBitisSaati { get; set; }
    }
}
