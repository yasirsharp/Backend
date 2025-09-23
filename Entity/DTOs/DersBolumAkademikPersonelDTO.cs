using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.DTOs
{
    public class DersBolumAkademikPersonelDTO:IDto
    {
        public int Id { get; set; }
        public int DersId { get; set; }
        public string DersAd { get; set; }
        public int BolumId { get; set; }
        public string BolumAd { get; set; }
        public int AkademikPersonelId { get; set; }
        public string AkademikPersonelAd { get; set; }
        public string Unvan { get; set; }
    }

}
