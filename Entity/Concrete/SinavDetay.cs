using Core.Entities;
using System;

namespace Entity.Concrete
{
    public class SinavDetay : IEntity
    {
        public int Id { get; set; }
        public int DersBolumAkademikPersonelId { get; set; }
        public DateTime SinavTarihi { get; set; }
        public TimeOnly SinavBaslangicSaati { get; set; }
        public TimeOnly SinavBitisSaati { get; set; }

        // Audit Alanları (IEntity'den gelir)
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Status { get; set; }
    }
}
