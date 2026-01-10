using Core.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entity.Concrete
{
    /// <summary>
    /// Akademik personelin müsait OLMADIĞI (meşgul) zaman dilimlerini tutar.
    /// Sınav gözetmenliği ataması yaparken bu kayıtlar kontrol edilir.
    /// Tekrar tipi ile haftalık/aylık düzenli meşguliyetler tanımlanabilir.
    /// </summary>
    public class AkademikPersonelMusaitlik : IEntity
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Müsaitlik kaydının ait olduğu akademik personel
        /// </summary>
        [Required]
        public int AkademikPersonelId { get; set; }

        /// <summary>
        /// Meşguliyetin başladığı tarih
        /// Tekrarlı kayıtlarda: İlk tekrar tarihi
        /// </summary>
        [Required]
        public DateTime BaslangicTarihi { get; set; }

        /// <summary>
        /// Meşguliyetin bitiş tarihi (tekrarlı kayıtlar için son tekrar tarihi)
        /// Null ise süresiz tekrar (dönem sonuna kadar vs.)
        /// Tekil kayıtlarda null olabilir (tek gün için)
        /// </summary>
        public DateTime? BitisTarihi { get; set; }

        /// <summary>
        /// Meşgul olunan başlangıç saati (null ise tüm gün meşgul)
        /// </summary>
        public TimeSpan? BaslangicSaati { get; set; }

        /// <summary>
        /// Meşgul olunan bitiş saati (null ise tüm gün meşgul)
        /// </summary>
        public TimeSpan? BitisSaati { get; set; }

        /// <summary>
        /// Tekrar tipi: 0=Tekil, 1=Haftalık, 2=Aylık
        /// </summary>
        [Required]
        public TekrarTipiEnum TekrarTipi { get; set; } = TekrarTipiEnum.Tekil;

        /// <summary>
        /// Haftalık tekrar için: Haftanın günü (0=Pazar, 1=Pazartesi, ..., 6=Cumartesi)
        /// Aylık tekrar için: Ayın günü (1-31)
        /// Tekil için: null
        /// </summary>
        public int? TekrarGunu { get; set; }

        /// <summary>
        /// Meşguliyet nedeni (opsiyonel)
        /// Örn: "Ders", "Toplantı", "İzin", "Kişisel"
        /// </summary>
        [MaxLength(100)]
        public string? Neden { get; set; }

        /// <summary>
        /// Ek açıklama
        /// </summary>
        [MaxLength(500)]
        public string? Aciklama { get; set; }

        // IEntity - Audit Alanları
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        public bool Status { get; set; } = true;

        // Navigation Property
        [ForeignKey("AkademikPersonelId")]
        public virtual AkademikPersonel? AkademikPersonel { get; set; }
    }

    /// <summary>
    /// Meşguliyet tekrar tipleri
    /// </summary>
    public enum TekrarTipiEnum
    {
        /// <summary>
        /// Sadece belirtilen tarih(ler)de geçerli
        /// </summary>
        Tekil = 0,

        /// <summary>
        /// Her hafta aynı günde tekrarlar
        /// </summary>
        Haftalik = 1,

        /// <summary>
        /// Her ay aynı günde tekrarlar
        /// </summary>
        Aylik = 2
    }
}
