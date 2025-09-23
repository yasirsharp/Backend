using Core.Entities;
using System.Collections.Generic;

namespace Entity.Concrete
{
    public class BolumAkademikPersoneller : IEntity
    {
        public int Id { get; set; }
        public int BolumId { get; set; }
        public int AkademikPersonelId { get; set; }
    }
} 