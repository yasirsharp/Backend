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
    public class EfSinavDerslikDal : EfEntityRepositoryBase<SinavDerslik, DuzceUniversiteContext>, ISinavDerslikDal
    {
        public List<SinavDetay> GetByDerslikId(int derslikId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from d in context.SinavDerslik
                             join s in context.SinavDetay on d.SinavDetayId equals s.Id
                             where d.DerslikId == derslikId
                             select new SinavDetay
                             {
                                 Id = s.Id,
                                 DersBolumAkademikPersonelId = s.DersBolumAkademikPersonelId,
                                 SinavBaslangicSaati = s.SinavBaslangicSaati,
                                 SinavBitisSaati = s.SinavBitisSaati,
                                 SinavTarihi = s.SinavTarihi
                             };
                return result.ToList();
            }
        }
        public List<SinavDetay> GetByGozetmenId(int GozetmenId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
              
                var result = from d in context.SinavDerslik
                             join s in context.SinavDetay on d.SinavDetayId equals s.Id
                             where d.GozetmenId == GozetmenId
                             select new SinavDetay
                             {
                                 Id = s.Id,
                                 DersBolumAkademikPersonelId = s.DersBolumAkademikPersonelId,
                                 SinavBaslangicSaati = s.SinavBaslangicSaati,
                                 SinavBitisSaati = s.SinavBitisSaati,
                                 SinavTarihi = s.SinavTarihi
                             };
                return result.ToList();
            }
        }

        public List<SinavDerslik> GetBySinavDetayId(int sinavDetayId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from d in context.SinavDerslik
                             join s in context.SinavDetay on d.SinavDetayId equals s.Id
                             where d.SinavDetayId == sinavDetayId
                             select new SinavDerslik
                             {
                                 Id = s.Id,
                                 DerslikId = d.DerslikId,
                                 GozetmenId = d.GozetmenId,
                                 SinavDetayId = d.SinavDetayId
                             };
                return result.ToList();
            }
        }
    }
}
