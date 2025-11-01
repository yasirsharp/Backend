using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entity.Concrete;
using Entity.DTOs;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfBolumAkademikPersonellerDal : EfEntityRepositoryBase<BolumAkademikPersoneller, DuzceUniversiteContext>, IBolumAkademikPersonellerDal
    {
        public BolumAkademikPersonelListDTO GetAkademikPersonellerByBolumId(int bolumId)
        {
            using (var context = new DuzceUniversiteContext())
            {
                var bolum = context.Bolum.FirstOrDefault(b => b.Id == bolumId);
                if (bolum == null) return null;

                var personelList = (from bap in context.BolumAkademikPersoneller
                                    join ap in context.AkademikPersonel on bap.AkademikPersonelId equals ap.Id
                                    where bap.BolumId == bolumId
                                    select new AkademikPersonelWithRelationship
                                    {
                                        Id = ap.Id,
                                        Ad = ap.Ad,
                                        Unvan = ap.Unvan,
                                        RelationshipId = bap.Id
                                    }).ToList();

                return new BolumAkademikPersonelListDTO
                {
                    BolumId = bolum.Id,
                    BolumAd = bolum.Ad,
                    AtanmisPersoneller = personelList
                };
            }
        }

        public AkademikPersonelBolumListDTO GetBolumlerByAkademikPersonelId(int akademikPersonelId)
        {
            using (var context = new DuzceUniversiteContext())
            {
                var personel = context.AkademikPersonel.FirstOrDefault(p => p.Id == akademikPersonelId);
                if (personel == null) return null;

                var bolumList = (from bap in context.BolumAkademikPersoneller
                                 join b in context.Bolum on bap.BolumId equals b.Id
                                 where bap.AkademikPersonelId == akademikPersonelId
                                 select new Bolum
                                 {
                                     Id = b.Id,
                                     Ad = b.Ad
                                 }).ToList();

                return new AkademikPersonelBolumListDTO
                {
                    AkademikPersonelId = personel.Id,
                    Ad = personel.Ad,
                    Unvan = personel.Unvan,
                    AtanmisBolumler = bolumList
                };
            }
        }
    }
}
