using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourProject.Models
{
    public class AkademikPersonelMusaitlik
    {
        public int Id { get; set; }

        public int AkademikPersonelId { get; set; }

        public DateTime BaslangicTarihi { get; set; }

        public DateTime BitisTarihi { get; set; }

        public TimeSpan? BaslangicSaati { get; set; }

        public TimeSpan? BitisSaati { get; set; }

        public string MusaitlikTipi { get; set; }

        public string Aciklama { get; set; }

        public string TekrarTipi { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool Status { get; set; }
    }
}