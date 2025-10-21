using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfDerslikBolumDal : EfEntityRepositoryBase<DerslikBolum, DuzceUniversiteContext>, IDerslikBolumDal
    {
        public List<DerslikBolum> GetByBolumId(int bolumId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                return context.DerslikBolum.Where(db => db.BolumId == bolumId).ToList();
            }
        }

        public List<DerslikBolum> GetByDerslikId(int derslikId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                return context.DerslikBolum.Where(db => db.DerslikId == derslikId).ToList();
            }
        }
    }
}
