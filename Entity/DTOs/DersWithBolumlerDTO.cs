using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.DTOs
{
    public class DersWithBolumlerDTO : IDto
    {
        public int DersId { get; set; }
        public string DersAd { get; set; }
        public bool OrtakDers { get; set; }
        public List<BolumInfo> Bolumler { get; set; } = new List<BolumInfo>();
    }

    public class BolumInfo
    {
        public int BolumId { get; set; }
        public string BolumAd { get; set; }
    }
}
