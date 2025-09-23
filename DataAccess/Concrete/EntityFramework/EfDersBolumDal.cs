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
    public class EfDersBolumDal : EfEntityRepositoryBase<DersBolum, DuzceUniversiteContext>, IDersBolumDal
    {
        public List<DersBolum> GetByBolumId(int bolumId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                return context.DersBolum.Where(db => db.BolumId == bolumId).ToList();
            }
        }

        public List<DersBolum> GetByDersId(int dersId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                return context.DersBolum.Where(db => db.DersId == dersId).ToList();
            }
        }
    }
}
