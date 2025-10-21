using Core.Entities;
using System;

namespace Entity.Concrete
{
    public class SinavDerslik : IEntity
    {
        public int Id { get; set; }
        public int DerslikId { get; set; }
        public int SinavDetayId { get; set; }
        public int GozetmenId { get; set; }

        // Audit Alanları (IEntity'den gelir)
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Status { get; set; }
    }
}
