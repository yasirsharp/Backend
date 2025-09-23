using Core.Entities;
using System;
using System.Collections.Generic;
using Entity.Concrete;

namespace Entity.DTOs
{
    public class SinavDetayDTO : IDto
    {
        public int Id { get; set; }
        public int DersBolumAkademikPersonelId { get; set; }
        public string DersAd { get; set; }
        public string BolumAd { get; set; }
        public string AkademikPersonelAd { get; set; }
        public string Unvan { get; set; }
        public int DerslikId { get; set; }
        public int GozetmenId { get; set; }
        public int DerslikKontenjan { get; set; }
        public DateTime SinavTarihi { get; set; }
        public TimeOnly SinavBaslangicSaati { get; set; }
        public TimeOnly SinavBitisSaati { get; set; }
        public List<DerslikGozetmenDTO> Derslikler { get; set; }
        public List<AkademikPersonel> Gozetmenler { get; set; }
    }
}
