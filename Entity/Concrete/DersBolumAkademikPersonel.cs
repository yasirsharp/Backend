using Core.Entities;

namespace Entity.Concrete
{
    public class DersBolumAkademikPersonel : IEntity
    {
        public int Id { get; set; }
        public int DersId { get; set; }
        public int BolumId { get; set; }
        public int AkademikPersonelId { get; set; }
    }
}
