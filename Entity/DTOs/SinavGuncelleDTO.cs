using Core.Entities;
using System;
using System.Collections.Generic;

namespace Entity.DTOs
{
    public class SinavGuncelleDTO : IDto
    {
        public int Id { get; set; }
        public int DerBolumAkademikPersonelId { get; set; }
        public DateTime SinavTarihi { get; set; }
        public TimeOnly SinavBaslangicSaati { get; set; }
        public TimeOnly SinavBitisSaati { get; set; }
        public List<DerslikGozetmenDTO> Derslikler { get; set; }
    }
} 