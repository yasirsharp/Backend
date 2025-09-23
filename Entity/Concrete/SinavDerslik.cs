using Core.Entities;

namespace Entity.Concrete
{
    public class SinavDerslik : IEntity
    {
        public int Id { get; set; }
        public int DerslikId { get; set; }
        public int SinavDetayId { get; set; }
        public int GozetmenId { get; set; }
    }
}
