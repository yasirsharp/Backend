using Core.Entities;
using System;

namespace Entity.Concrete
{
    /// <summary>
    /// Görevli personelin (gorevli.personel) bilgi görebileceği bölümler. Bir kişi birden çok bölüme atanabilir.
    /// Bölüm görevlilerinin bölümleri ayrıca BolumAkademikPersoneller tablosundadır.
    /// </summary>
    public class KullaniciBolum : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BolumId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Status { get; set; } = true;
    }
}
