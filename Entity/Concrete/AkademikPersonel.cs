using Core.Entities;
using System;

namespace Entity.Concrete
{
    public class AkademikPersonel : IEntity
    {
        public int Id { get; set; }
        public string Ad { get; set; }
        public string Unvan { get; set; }
        public int UserId { get; set; }

        // Audit Alanları (IEntity'den gelir)
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Status { get; set; }
    }
}
