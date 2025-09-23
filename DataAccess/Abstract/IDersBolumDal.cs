using Core.DataAccess;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface IDersBolumDal : IEntityRepository<DersBolum>
    {
        List<DersBolum> GetByDersId(int dersId);
        List<DersBolum> GetByBolumId(int bolumId);
    }
}
