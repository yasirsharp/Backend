using Core.Entities;
using System;

namespace Entity.Concrete
{
    public class DersBolumAkademikPersonel : IEntity
    {
        public int Id { get; set; }
        public int DersId { get; set; }
        public int BolumId { get; set; }
        public int AkademikPersonelId { get; set; }

        // Audit Alanları (IEntity'den gelir)
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Status { get; set; }
    }
}
