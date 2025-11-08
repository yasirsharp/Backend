using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.DTOs
{
    public class DerslikWithBolumlerDTO : IDto
    {
        public int DerslikId { get; set; }
        public string DerslikAd { get; set; } = null!;
        public int Kapasite { get; set; }
        public bool OrtakDerslik { get; set; }
        public bool Status { get; set; }
        public List<BolumInfo> Bolumler { get; set; } = new List<BolumInfo>();
    }
}
