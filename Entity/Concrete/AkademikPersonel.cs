using Core.Entities;

namespace Entity.Concrete
{
    public class AkademikPersonel : IEntity
    {
        public int Id { get; set; }
        public string Ad { get; set; }
        public string Unvan { get; set; }
        public int UserId { get; set; }
    }
}
