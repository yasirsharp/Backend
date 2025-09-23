using Core.Entities;

namespace Entity.Concrete
{
    public class Derslik : IEntity
    {
        public int Id { get; set; }
        public string Ad { get; set; }
        public int Kapasite { get; set; }
    }
}
