using Core.DataAccess;
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entity.Concrete;
using Entity.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfSinavDetayDal : EfEntityRepositoryBase<SinavDetay, DuzceUniversiteContext>, ISinavDetayDal
    {
        public void AddWithTransaction(SinavKayitDTO sinavKayitDTO)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        // Saat dönüþümleri
                        TimeOnly baslangicSaati = TimeOnly.Parse(sinavKayitDTO.SinavBaslangicSaati);
                        TimeOnly bitisSaati = TimeOnly.Parse(sinavKayitDTO.SinavBitisSaati);

                        SinavDetay sinavDetay = new SinavDetay
                        {
                            DersBolumAkademikPersonelId = sinavKayitDTO.DersBolumAkademikPersonelId,
                            SinavTarihi = sinavKayitDTO.SinavTarihi,
                            SinavBaslangicSaati = baslangicSaati,
                            SinavBitisSaati = bitisSaati
                        };
                        context.SinavDetay.Add(sinavDetay);
                        context.SaveChanges();

                        foreach (var derslik in sinavKayitDTO.Derslikler)
                        {
                            SinavDerslik sinavDerslik = new SinavDerslik
                            {
                                SinavDetayId = sinavDetay.Id,
                                DerslikId = derslik.DerslikId,
                                GozetmenId = derslik.GozetmenId ?? 0 
                            };
                            context.SinavDerslik.Add(sinavDerslik);
                        }

                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception err)
                    {
                        transaction.Rollback();
                        throw new Exception(err.Message);
                    }
                }
            }
        }

        public void DeleteWithTransaction(SinavDetay sinavDetay)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var sinavDerslikler = context.SinavDerslik.Where(x => x.SinavDetayId == sinavDetay.Id);
                        context.SinavDerslik.RemoveRange(sinavDerslikler);
                        
                        context.SinavDetay.Remove(sinavDetay);
                        context.SaveChanges();
                        
                        transaction.Commit();
                    }
                    catch (Exception err)
                    {
                        transaction.Rollback();
                        throw err;
                    }
                }
            }
        }
        public SinavDetay ExistSinav(List<int> derslikIdleri, List<int> gozetmenIdleri, int akademikPersonelId, TimeOnly SinavBaslangicSaati, TimeOnly SinavBitisSaati, DateTime sinavTarihi)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from s in context.SinavDetay
                             join sd in context.SinavDerslik on s.Id equals sd.SinavDetayId into sd_join
                             from sd in sd_join.DefaultIfEmpty()
                             join dba in context.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals dba.Id into dba_join
                             from dba in dba_join.DefaultIfEmpty()
                             where s.SinavTarihi.Date == sinavTarihi.Date &&
                                   (
                                       // Derslik çakýþma kontrolü
                                       (derslikIdleri.Contains(sd.DerslikId)) ||
                                       // Gözetmen çakýþma kontrolü
                                       (gozetmenIdleri.Contains(sd.GozetmenId)) ||
                                       // Akademik personel çakýþma kontrolü
                                       (akademikPersonelId == dba.AkademikPersonelId)
                                   ) &&
                                   (
                                       (s.SinavBaslangicSaati <= SinavBaslangicSaati && s.SinavBitisSaati > SinavBaslangicSaati) ||
                                       (s.SinavBaslangicSaati < SinavBitisSaati && s.SinavBitisSaati >= SinavBitisSaati) ||
                                       (s.SinavBaslangicSaati >= SinavBaslangicSaati && s.SinavBitisSaati <= SinavBitisSaati)
                                   )
                             select s;

                return result.FirstOrDefault();
            }
        }

        public List<SinavDetayDTO> GetByDerslikler(int[] derslikIds)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from s in context.SinavDetay
                             join dba in context.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals dba.Id
                             join d in context.Ders on dba.DersId equals d.Id
                             join b in context.Bolum on dba.BolumId equals b.Id
                             join ap in context.AkademikPersonel on dba.AkademikPersonelId equals ap.Id
                             join sd in context.SinavDerslik on s.Id equals sd.SinavDetayId
                             join dlik in context.Derslik on sd.DerslikId equals dlik.Id
                             where derslikIds.Contains(sd.DerslikId)
                             select new SinavDetayDTO
                             {
                                 Id = s.Id,
                                 DersAd = d.Ad,
                                 BolumId = b.Id,
                                 BolumAd = b.Ad,
                                 AkademikPersonelAd = ap.Ad,
                                 Unvan = ap.Unvan,
                                 SinavTarihi = s.SinavTarihi,
                                 SinavBaslangicSaati = s.SinavBaslangicSaati,
                                 SinavBitisSaati = s.SinavBitisSaati,
                                 DerslikId = sd.DerslikId,
                                 DerslikKontenjan = dlik.Kapasite,
                                 GozetmenId = sd.GozetmenId,
                                 DersBolumAkademikPersonelId = dba.Id,
                                 Derslikler = context.SinavDerslik
                                     .Where(x => x.SinavDetayId == s.Id)
                                     .Select(x => new DerslikGozetmenDTO
                                     {
                                         DerslikId = x.DerslikId,
                                         GozetmenId = x.GozetmenId
                                     }).ToList(),
                                 Gozetmenler = context.SinavDerslik
                                     .Where(x => x.SinavDetayId == s.Id)
                                     .Join(context.AkademikPersonel,
                                         sd => sd.GozetmenId,
                                         ap => ap.Id,
                                         (sd, ap) => ap)
                                     .ToList()
                             };

                return result.Distinct().ToList();
            }
        }

        public List<SinavDetayDTO> GetByDersliklerAndBolum(int[] derslikIds, int bolumId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from s in context.SinavDetay
                             join dba in context.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals dba.Id
                             join d in context.Ders on dba.DersId equals d.Id
                             join b in context.Bolum on dba.BolumId equals b.Id
                             join ap in context.AkademikPersonel on dba.AkademikPersonelId equals ap.Id
                             join sd in context.SinavDerslik on s.Id equals sd.SinavDetayId
                             join dlik in context.Derslik on sd.DerslikId equals dlik.Id
                             where derslikIds.Contains(sd.DerslikId) && b.Id == bolumId
                             select new SinavDetayDTO
                             {
                                 Id = s.Id,
                                 DersAd = d.Ad,
                                 BolumId = b.Id,
                                 BolumAd = b.Ad,
                                 AkademikPersonelAd = ap.Ad,
                                 Unvan = ap.Unvan,
                                 SinavTarihi = s.SinavTarihi,
                                 SinavBaslangicSaati = s.SinavBaslangicSaati,
                                 SinavBitisSaati = s.SinavBitisSaati,
                                 DerslikId = sd.DerslikId,
                                 DerslikKontenjan = dlik.Kapasite,
                                 GozetmenId = sd.GozetmenId,
                                 DersBolumAkademikPersonelId = dba.Id,
                                 Derslikler = context.SinavDerslik
                             .Where(x => x.SinavDetayId == s.Id)
                             .Select(x => new DerslikGozetmenDTO
                             {
                                 DerslikId = x.DerslikId,
                                 GozetmenId = x.GozetmenId
                             }).ToList(),
                                 Gozetmenler = context.SinavDerslik
                             .Where(x => x.SinavDetayId == s.Id)
                             .Join(context.AkademikPersonel,
                                 sd => sd.GozetmenId,
                                 ap => ap.Id,
                                 (sd, ap) => ap)
                             .ToList()
                             };

                return result.Distinct().ToList();
            }
        }

        public List<SinavDetayDTO> GetByDersliklerAndAkademikPersonel(int[] derslikIds, int akademikPersonelId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from s in context.SinavDetay
                             join dba in context.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals dba.Id
                             join d in context.Ders on dba.DersId equals d.Id
                             join b in context.Bolum on dba.BolumId equals b.Id
                             join ap in context.AkademikPersonel on dba.AkademikPersonelId equals ap.Id
                             join sd in context.SinavDerslik on s.Id equals sd.SinavDetayId
                             join dlik in context.Derslik on sd.DerslikId equals dlik.Id
                             where derslikIds.Contains(sd.DerslikId) &&
                                   (dba.AkademikPersonelId == akademikPersonelId || sd.GozetmenId == akademikPersonelId)
                             select new SinavDetayDTO
                             {
                                 Id = s.Id,
                                 DersAd = d.Ad,
                                 BolumId = b.Id,
                                 BolumAd = b.Ad,
                                 AkademikPersonelAd = ap.Ad,
                                 Unvan = ap.Unvan,
                                 SinavTarihi = s.SinavTarihi,
                                 SinavBaslangicSaati = s.SinavBaslangicSaati,
                                 SinavBitisSaati = s.SinavBitisSaati,
                                 DerslikId = sd.DerslikId,
                                 DerslikKontenjan = dlik.Kapasite,
                                 GozetmenId = sd.GozetmenId,
                                 DersBolumAkademikPersonelId = dba.Id,
                                 Derslikler = context.SinavDerslik
                             .Where(x => x.SinavDetayId == s.Id)
                             .Select(x => new DerslikGozetmenDTO
                             {
                                 DerslikId = x.DerslikId,
                                 GozetmenId = x.GozetmenId
                             }).ToList(),
                                 Gozetmenler = context.SinavDerslik
                             .Where(x => x.SinavDetayId == s.Id)
                             .Join(context.AkademikPersonel,
                                 sd => sd.GozetmenId,
                                 ap => ap.Id,
                                 (sd, ap) => ap)
                             .ToList()
                             };

                return result.Distinct().ToList();
            }
        }

        public List<SinavDetayDTO> GetByBolumId(int bolumId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from s in context.SinavDetay
                             join dba in context.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals dba.Id
                             join d in context.Ders on dba.DersId equals d.Id
                             join b in context.Bolum on dba.BolumId equals b.Id
                             join ap in context.AkademikPersonel on dba.AkademikPersonelId equals ap.Id
                             join sd in context.SinavDerslik on s.Id equals sd.SinavDetayId
                             join dlik in context.Derslik on sd.DerslikId equals dlik.Id
                             where b.Id == bolumId
                             select new SinavDetayDTO
                             {
                                 Id = s.Id,
                                 DersAd = d.Ad,
                                 BolumId = b.Id,
                                 BolumAd = b.Ad,
                                 AkademikPersonelAd = ap.Ad,
                                 Unvan = ap.Unvan,
                                 SinavTarihi = s.SinavTarihi,
                                 SinavBaslangicSaati = s.SinavBaslangicSaati,
                                 DerslikId = sd.DerslikId,
                                 DerslikKontenjan = dlik.Kapasite,
                                 GozetmenId = sd.GozetmenId,
                                 DersBolumAkademikPersonelId = dba.Id,
                                 SinavBitisSaati = s.SinavBitisSaati,
                                 Derslikler = context.SinavDerslik
                                     .Where(x => x.SinavDetayId == s.Id)
                                     .Select(x => new DerslikGozetmenDTO
                                     {
                                         DerslikId = x.DerslikId,
                                         GozetmenId = x.GozetmenId
                                     }).ToList(),
                                 Gozetmenler = context.SinavDerslik
                                     .Where(x => x.SinavDetayId == s.Id)
                                     .Join(context.AkademikPersonel,
                                         sd => sd.GozetmenId,
                                         ap => ap.Id,
                                         (sd, ap) => ap)
                                     .ToList()
                             };

                return result.ToList();
            }
        }

        public SinavDetayDTO GetDetail(int sinavDetayId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from s in context.SinavDetay
                             join dba in context.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals dba.Id
                             join d in context.Ders on dba.DersId equals d.Id
                             join b in context.Bolum on dba.BolumId equals b.Id
                             join ap in context.AkademikPersonel on dba.AkademikPersonelId equals ap.Id
                             join sd in context.SinavDerslik on s.Id equals sd.SinavDetayId
                             where s.Id == sinavDetayId
                             select new SinavDetayDTO
                             {
                                 Id = s.Id,
                                 DersAd = d.Ad,
                                 BolumId = b.Id,
                                 BolumAd = b.Ad,
                                 DerslikId = sd.DerslikId,
                                 GozetmenId = sd.GozetmenId,
                                 AkademikPersonelAd = ap.Ad,
                                 Unvan = ap.Unvan,
                                 SinavTarihi = s.SinavTarihi,
                                 SinavBaslangicSaati = s.SinavBaslangicSaati,
                                 SinavBitisSaati = s.SinavBitisSaati,
                                 DersBolumAkademikPersonelId = dba.Id,
                                 Derslikler = context.SinavDerslik
                                     .Where(x => x.SinavDetayId == s.Id)
                                     .Select(x => new DerslikGozetmenDTO
                                     {
                                         DerslikId = x.DerslikId,
                                         GozetmenId = x.GozetmenId
                                     }).ToList(),
                                 Gozetmenler = context.SinavDerslik
                                     .Where(x => x.SinavDetayId == s.Id)
                                     .Join(context.AkademikPersonel,
                                         sd => sd.GozetmenId,
                                         ap => ap.Id,
                                         (sd, ap) => ap)
                                     .ToList()
                             };

                return result.FirstOrDefault()!;
            }
        }

        public List<SinavDetayDTO> GetDetails()
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from s in context.SinavDetay
                             join dba in context.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals dba.Id
                             join d in context.Ders on dba.DersId equals d.Id
                             join b in context.Bolum on dba.BolumId equals b.Id
                             join ap in context.AkademikPersonel on dba.AkademikPersonelId equals ap.Id
                             join sd in context.SinavDerslik on s.Id equals sd.SinavDetayId
                             join dlik in context.Derslik on sd.DerslikId equals dlik.Id
                             select new SinavDetayDTO
                             {
                                 Id = s.Id,
                                 DersAd = d.Ad,
                                 BolumId = b.Id,
                                 BolumAd = b.Ad,
                                 AkademikPersonelAd = ap.Ad,
                                 Unvan = ap.Unvan,
                                 SinavTarihi = s.SinavTarihi,
                                 SinavBaslangicSaati = s.SinavBaslangicSaati,
                                 SinavBitisSaati = s.SinavBitisSaati,
                                 DerslikId = sd.DerslikId,
                                 DerslikKontenjan = dlik.Kapasite,
                                 GozetmenId = sd.GozetmenId,
                                 DersBolumAkademikPersonelId = dba.Id,
                                 Derslikler = context.SinavDerslik
                                     .Where(x => x.SinavDetayId == s.Id)
                                     .Select(x => new DerslikGozetmenDTO
                                     {
                                         DerslikId = x.DerslikId,
                                         GozetmenId = x.GozetmenId
                                     }).ToList(),
                                 Gozetmenler = context.SinavDerslik
                                     .Where(x => x.SinavDetayId == s.Id)
                                     .Join(context.AkademikPersonel,
                                         sd => sd.GozetmenId,
                                         ap => ap.Id,
                                         (sd, ap) => ap)
                                     .ToList()
                             };
                return result.ToList();
            }
        }

        public void UpdateWithTransaction(SinavGuncelleDTO sinavGuncelleDTO)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var sinavDetay = context.SinavDetay.Find(sinavGuncelleDTO.Id);
                        if (sinavDetay != null)
                        {
                            sinavDetay.DersBolumAkademikPersonelId = sinavGuncelleDTO.DersBolumAkademikPersonelId;
                            sinavDetay.SinavTarihi = sinavGuncelleDTO.SinavTarihi;
                            sinavDetay.SinavBaslangicSaati = sinavGuncelleDTO.SinavBaslangicSaati;
                            sinavDetay.SinavBitisSaati = sinavGuncelleDTO.SinavBitisSaati;

                            var existingSinavDerslikler = context.SinavDerslik.Where(x => x.SinavDetayId == sinavGuncelleDTO.Id);
                            context.SinavDerslik.RemoveRange(existingSinavDerslikler);

                            foreach (var derslik in sinavGuncelleDTO.Derslikler)
                            {
                                SinavDerslik sinavDerslik = new SinavDerslik
                                {
                                    SinavDetayId = sinavDetay.Id,
                                    DerslikId = derslik.DerslikId,
                                    GozetmenId = derslik.GozetmenId ?? 0
                                };
                                context.SinavDerslik.Add(sinavDerslik);
                            }

                            context.SaveChanges();
                            transaction.Commit();
                        }
                    }
                    catch (Exception err)
                    {
                        transaction.Rollback();
                        throw err;
                    }
                }
            }
        }

        public List<SinavDetayDTO> GetByDerslikId(int derslikId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from s in context.SinavDetay
                             join dba in context.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals dba.Id
                             join d in context.Ders on dba.DersId equals d.Id
                             join b in context.Bolum on dba.BolumId equals b.Id
                             join ap in context.AkademikPersonel on dba.AkademikPersonelId equals ap.Id
                             join sd in context.SinavDerslik on s.Id equals sd.SinavDetayId
                             join dlik in context.Derslik on sd.DerslikId equals dlik.Id
                             where sd.DerslikId == derslikId
                             select new SinavDetayDTO
                             {
                                 Id = s.Id,
                                 DersAd = d.Ad,
                                 BolumId = b.Id,
                                 BolumAd = b.Ad,
                                 AkademikPersonelAd = ap.Ad,
                                 Unvan = ap.Unvan,
                                 SinavTarihi = s.SinavTarihi,
                                 SinavBaslangicSaati = s.SinavBaslangicSaati,
                                 DerslikId = sd.DerslikId,
                                 DerslikKontenjan = dlik.Kapasite,
                                 GozetmenId = sd.GozetmenId,
                                 DersBolumAkademikPersonelId = dba.Id,
                                 SinavBitisSaati = s.SinavBitisSaati,
                                 Derslikler = context.SinavDerslik
                                     .Where(x => x.SinavDetayId == s.Id)
                                     .Select(x => new DerslikGozetmenDTO
                                     {
                                         DerslikId = x.DerslikId,
                                         GozetmenId = x.GozetmenId
                                     }).ToList(),
                                 Gozetmenler = context.SinavDerslik
                                     .Where(x => x.SinavDetayId == s.Id)
                                     .Join(context.AkademikPersonel,
                                         sd => sd.GozetmenId,
                                         ap => ap.Id,
                                         (sd, ap) => ap)
                                     .ToList()
                             };

                return result.ToList();
            }
        }
        
        public List<SinavDetayDTO> GetByAkademikPersonelId(int akademikPersonelId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from s in context.SinavDetay
                             join dba in context.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals dba.Id
                             join d in context.Ders on dba.DersId equals d.Id
                             join b in context.Bolum on dba.BolumId equals b.Id
                             join ap in context.AkademikPersonel on dba.AkademikPersonelId equals ap.Id
                             join sd in context.SinavDerslik on s.Id equals sd.SinavDetayId
                             join dlik in context.Derslik on sd.DerslikId equals dlik.Id
                             where dba.AkademikPersonelId == akademikPersonelId || sd.GozetmenId == akademikPersonelId
                             select new SinavDetayDTO
                             {
                                 Id = s.Id,
                                 DersAd = d.Ad,
                                 BolumId = b.Id,
                                 BolumAd = b.Ad,
                                 AkademikPersonelAd = ap.Ad,
                                 Unvan = ap.Unvan,
                                 SinavTarihi = s.SinavTarihi,
                                 SinavBaslangicSaati = s.SinavBaslangicSaati,
                                 DerslikId = sd.DerslikId,
                                 DerslikKontenjan = dlik.Kapasite,
                                 GozetmenId = sd.GozetmenId,
                                 DersBolumAkademikPersonelId = dba.Id,
                                 SinavBitisSaati = s.SinavBitisSaati,
                                 Derslikler = context.SinavDerslik
                                     .Where(x => x.SinavDetayId == s.Id)
                                     .Select(x => new DerslikGozetmenDTO
                                     {
                                         DerslikId = x.DerslikId,
                                         GozetmenId = x.GozetmenId
                                     }).ToList(),
                                 Gozetmenler = context.SinavDerslik
                                     .Where(x => x.SinavDetayId == s.Id)
                                     .Join(context.AkademikPersonel,
                                         sd => sd.GozetmenId,
                                         ap => ap.Id,
                                         (sd, ap) => ap)
                                     .ToList()
                             };

                return result.Distinct().ToList();
            }
        }

        public List<SinavDetayDTO> GetByDBAPId(int dbapId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from s in context.SinavDetay
                             join dba in context.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals dba.Id
                             join d in context.Ders on dba.DersId equals d.Id
                             join b in context.Bolum on dba.BolumId equals b.Id
                             join ap in context.AkademikPersonel on dba.AkademikPersonelId equals ap.Id
                             join sd in context.SinavDerslik on s.Id equals sd.SinavDetayId
                             join dlik in context.Derslik on sd.DerslikId equals dlik.Id
                             where dba.Id == dbapId
                             select new SinavDetayDTO
                             {
                                 Id = s.Id,
                                 DersAd = d.Ad,
                                 BolumId = b.Id,
                                 BolumAd = b.Ad,
                                 AkademikPersonelAd = ap.Ad,
                                 Unvan = ap.Unvan,
                                 SinavTarihi = s.SinavTarihi,
                                 SinavBaslangicSaati = s.SinavBaslangicSaati,
                                 DerslikId = sd.DerslikId,
                                 DerslikKontenjan = dlik.Kapasite,
                                 GozetmenId = sd.GozetmenId,
                                 DersBolumAkademikPersonelId = dba.Id,
                                 SinavBitisSaati = s.SinavBitisSaati,
                                 Derslikler = context.SinavDerslik
                                     .Where(x => x.SinavDetayId == s.Id)
                                     .Select(x => new DerslikGozetmenDTO
                                     {
                                         DerslikId = x.DerslikId,
                                         GozetmenId = x.GozetmenId
                                     }).ToList(),
                                 Gozetmenler = context.SinavDerslik
                                     .Where(x => x.SinavDetayId == s.Id)
                                     .Join(context.AkademikPersonel,
                                         sd => sd.GozetmenId,
                                         ap => ap.Id,
                                         (sd, ap) => ap)
                                     .ToList()
                             };

                return result.Distinct().ToList();
            }
        }

        public List<SinavDetayDTO> GetSinavDetailsByDateRange(DateTime startDate, DateTime endDate)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from s in context.SinavDetay
                             join dba in context.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals dba.Id
                             join d in context.Ders on dba.DersId equals d.Id
                             join b in context.Bolum on dba.BolumId equals b.Id
                             join ap in context.AkademikPersonel on dba.AkademikPersonelId equals ap.Id
                             where s.SinavTarihi >= startDate && s.SinavTarihi <= endDate
                             select new SinavDetayDTO
                             {
                                 Id = s.Id,
                                 DersAd = d.Ad,
                                 BolumId = b.Id,
                                 BolumAd = b.Ad,
                                 AkademikPersonelAd = ap.Ad,
                                 Unvan = ap.Unvan,
                                 SinavTarihi = s.SinavTarihi,
                                 SinavBaslangicSaati = s.SinavBaslangicSaati,
                                 SinavBitisSaati = s.SinavBitisSaati,
                                 DersBolumAkademikPersonelId = dba.Id,
                                 Derslikler = context.SinavDerslik
                                    .Where(x => x.SinavDetayId == s.Id)
                                    .Select(x => new DerslikGozetmenDTO
                                    {
                                        DerslikId = x.DerslikId,
                                        GozetmenId = x.GozetmenId
                                    }).ToList(),
                                 Gozetmenler = context.SinavDerslik
                                    .Where(x => x.SinavDetayId == s.Id)
                                    .Join(context.AkademikPersonel,
                                        sd => sd.GozetmenId,
                                        ap => ap.Id,
                                        (sd, ap) => ap)
                                    .ToList()
                             };

                return result.ToList();
            }
        }

        public List<SinavDetayDTO> GetSinavDetailsByDateRangeAndBolum(DateTime startDate, DateTime endDate, int bolumId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from s in context.SinavDetay
                             join dba in context.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals dba.Id
                             join d in context.Ders on dba.DersId equals d.Id
                             join b in context.Bolum on dba.BolumId equals b.Id
                             join ap in context.AkademikPersonel on dba.AkademikPersonelId equals ap.Id
                             where s.SinavTarihi >= startDate && s.SinavTarihi <= endDate
                                   && b.Id == bolumId
                             select new SinavDetayDTO
                             {
                                 Id = s.Id,
                                 DersAd = d.Ad,
                                 BolumId = b.Id,
                                 BolumAd = b.Ad,
                                 AkademikPersonelAd = ap.Ad,
                                 Unvan = ap.Unvan,
                                 SinavTarihi = s.SinavTarihi,
                                 SinavBaslangicSaati = s.SinavBaslangicSaati,
                                 SinavBitisSaati = s.SinavBitisSaati,
                                 DersBolumAkademikPersonelId = dba.Id,
                                 Derslikler = context.SinavDerslik
                                    .Where(x => x.SinavDetayId == s.Id)
                                    .Select(x => new DerslikGozetmenDTO
                                    {
                                        DerslikId = x.DerslikId,
                                        GozetmenId = x.GozetmenId
                                    }).ToList(),
                                 Gozetmenler = context.SinavDerslik
                                    .Where(x => x.SinavDetayId == s.Id)
                                    .Join(context.AkademikPersonel,
                                        sd => sd.GozetmenId,
                                        ap => ap.Id,
                                        (sd, ap) => ap)
                                    .ToList()
                             };

                return result.ToList();
            }
        }

        public List<SinavDetayDTO> GetSinavDetailsByDateRangeAndDerslik(DateTime startDate, DateTime endDate, int derslikId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from s in context.SinavDetay
                             join dba in context.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals dba.Id
                             join d in context.Ders on dba.DersId equals d.Id
                             join b in context.Bolum on dba.BolumId equals b.Id
                             join ap in context.AkademikPersonel on dba.AkademikPersonelId equals ap.Id
                             join sd in context.SinavDerslik on s.Id equals sd.SinavDetayId
                             where s.SinavTarihi >= startDate && s.SinavTarihi <= endDate
                                   && sd.DerslikId == derslikId
                             select new SinavDetayDTO
                             {
                                 Id = s.Id,
                                 DersAd = d.Ad,
                                 BolumId = b.Id,
                                 BolumAd = b.Ad,
                                 AkademikPersonelAd = ap.Ad,
                                 Unvan = ap.Unvan,
                                 SinavTarihi = s.SinavTarihi,
                                 SinavBaslangicSaati = s.SinavBaslangicSaati,
                                 SinavBitisSaati = s.SinavBitisSaati,
                                 DersBolumAkademikPersonelId = dba.Id,
                                 Derslikler = context.SinavDerslik
                                    .Where(x => x.SinavDetayId == s.Id)
                                    .Select(x => new DerslikGozetmenDTO
                                    {
                                        DerslikId = x.DerslikId,
                                        GozetmenId = x.GozetmenId
                                    }).ToList(),
                                 Gozetmenler = context.SinavDerslik
                                    .Where(x => x.SinavDetayId == s.Id)
                                    .Join(context.AkademikPersonel,
                                        sd => sd.GozetmenId,
                                        ap => ap.Id,
                                        (sd, ap) => ap)
                                    .ToList()
                             };

                return result.Distinct().ToList();
            }
        }

        public List<SinavDetayDTO> GetSinavDetailsByDateRangeAndAkademikPersonel(DateTime startDate, DateTime endDate, int akademikPersonelId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from s in context.SinavDetay
                             join dba in context.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals dba.Id
                             join d in context.Ders on dba.DersId equals d.Id
                             join b in context.Bolum on dba.BolumId equals b.Id
                             join ap in context.AkademikPersonel on dba.AkademikPersonelId equals ap.Id
                             join sd in context.SinavDerslik on s.Id equals sd.SinavDetayId
                             where s.SinavTarihi >= startDate && s.SinavTarihi <= endDate
                                   && (dba.AkademikPersonelId == akademikPersonelId || sd.GozetmenId == akademikPersonelId)
                             select new SinavDetayDTO
                             {
                                 Id = s.Id,
                                 DersAd = d.Ad,
                                 BolumId = b.Id,
                                 BolumAd = b.Ad,
                                 AkademikPersonelAd = ap.Ad,
                                 Unvan = ap.Unvan,
                                 SinavTarihi = s.SinavTarihi,
                                 SinavBaslangicSaati = s.SinavBaslangicSaati,
                                 SinavBitisSaati = s.SinavBitisSaati,
                                 DersBolumAkademikPersonelId = dba.Id,
                                 Derslikler = context.SinavDerslik
                                    .Where(x => x.SinavDetayId == s.Id)
                                    .Select(x => new DerslikGozetmenDTO
                                    {
                                        DerslikId = x.DerslikId,
                                        GozetmenId = x.GozetmenId
                                    }).ToList(),
                                 Gozetmenler = context.SinavDerslik
                                    .Where(x => x.SinavDetayId == s.Id)
                                    .Join(context.AkademikPersonel,
                                        sd => sd.GozetmenId,
                                        ap => ap.Id,
                                        (sd, ap) => ap)
                                    .ToList()
                             };

                return result.Distinct().ToList();
            }
        }
    }
}
