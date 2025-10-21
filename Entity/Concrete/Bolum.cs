using Core.Entities;
using System;

namespace Entity.Concrete
{
    public class Bolum : IEntity
    {
        public int Id { get; set; }
        public string Ad { get; set; }

        // Audit Alanları (IEntity'den gelir)
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Status { get; set; }
    }
}
