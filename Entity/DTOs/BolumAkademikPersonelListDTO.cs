using Core.Entities;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.DTOs
{
    public class BolumAkademikPersonelListDTO : IDto
    {
        public int BolumId { get; set; }
        public string BolumAd { get; set; }
        public List<AkademikPersonelWithRelationship> AtanmisPersoneller { get; set; }
    }
}
