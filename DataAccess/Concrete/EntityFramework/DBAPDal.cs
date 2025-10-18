using Core.DataAccess.EntityFramework;
using Core.Entities;
using DataAccess.Abstract;
using Entity.Concrete;
using Entity.DTOs;
using System.Linq;
using System.Linq.Expressions;

namespace DataAccess.Concrete.EntityFramework
{
    public class DBAPDal : EfEntityRepositoryBase<DersBolumAkademikPersonel, DuzceUniversiteContext>, IDBAPDal
    {
        public DersBolumAkademikPersonelDTO GetDetail(int dbapId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from dba in context.DersBolumAkademikPersonel
                             join d in context.Ders on dba.DersId equals d.Id
                             join b in context.Bolum on dba.BolumId equals b.Id
                             join ap in context.AkademikPersonel on dba.AkademikPersonelId equals ap.Id
                             where dba.Id == dbapId
                             select new DersBolumAkademikPersonelDTO
                             {
                                 Id = dba.Id,
                                 DersId = d.Id,
                                 DersAd = d.Ad,
                                 DersKodu = d.Kod,
                                 BolumId = b.Id,
                                 BolumAd = b.Ad,
                                 AkademikPersonelId = ap.Id,
                                 AkademikPersonelAd = ap.Ad,
                                 Unvan = ap.Unvan
                             };
                return result.FirstOrDefault();
            }
        }

        public List<DersBolumAkademikPersonelDTO> GetDetails(Expression<Func<DersBolumAkademikPersonelDTO, bool>> filter = null)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from dba in context.DersBolumAkademikPersonel
                             join d in context.Ders on dba.DersId equals d.Id
                             join b in context.Bolum on dba.BolumId equals b.Id
                             join ap in context.AkademikPersonel on dba.AkademikPersonelId equals ap.Id
                             select new DersBolumAkademikPersonelDTO
                             {
                                 Id = dba.Id,
                                 DersId = d.Id,
                                 DersAd = d.Ad,
                                 DersKodu = d.Kod,
                                 BolumId = b.Id,
                                 BolumAd = b.Ad,
                                 AkademikPersonelId = ap.Id,
                                 AkademikPersonelAd = ap.Ad,
                                 Unvan = ap.Unvan
                             };
                return filter == null
                        ? result.ToList()
                        : result.Where(filter).ToList();
            }
        }
    }
}
