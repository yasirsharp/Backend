using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Concrete
{
    public class DersBolum : IEntity
    {
        public int Id { get; set; }
        public int DersId { get; set; }
        public int BolumId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
