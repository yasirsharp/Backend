using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.DTOs
{
    public class DersEkleDTO : IDto
    {
        public string Ad { get; set; } = null!;
        public string Kod { get; set; } = null!;
        public bool OrtakDers { get; set; }
        public List<int> BolumIds { get; set; } = new List<int>();
    }
}
