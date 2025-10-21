using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.DTOs
{
    public class DerslikGuncelleDTO : IDto
    {
        public int Id { get; set; }
        public string Ad { get; set; } = null!;
        public int Kapasite { get; set; }
        public bool OrtakDerslik { get; set; }
        public List<int> BolumIds { get; set; } = new List<int>();
    }
}
