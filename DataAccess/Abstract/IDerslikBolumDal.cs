using Core.DataAccess;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface IDerslikBolumDal : IEntityRepository<DerslikBolum>
    {
        List<DerslikBolum> GetByDerslikId(int derslikId);
        List<DerslikBolum> GetByBolumId(int bolumId);
    }
}
