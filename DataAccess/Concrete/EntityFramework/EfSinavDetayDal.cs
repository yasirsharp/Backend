using Core.DataAccess;
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entity.Concrete;
using Entity.DTOs;
using Microsoft.EntityFrameworkCore;
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
        /// <summary>
        /// Çakışma kontrolü ve kayıt aynı transaction ve aynı uygulama kilidi altında yapılır.
        /// Çakışma varsa kayıt eklenmez ve çakışan sınav döner; yoksa sınav eklenir ve null döner.
        /// </summary>
        public SinavDetay? AddIfNoConflict(SinavKayitDTO sinavKayitDTO, out int createdId)
        {
            createdId = 0;
            using DuzceUniversiteContext context = new DuzceUniversiteContext();
            using var transaction = context.Database.BeginTransaction();

            using var planlamaKilidi = AcquireSinavPlanlamaLock(context);

            // Saat dönüşümleri
            TimeOnly baslangicSaati = TimeOnly.Parse(sinavKayitDTO.SinavBaslangicSaati);
            TimeOnly bitisSaati = TimeOnly.Parse(sinavKayitDTO.SinavBitisSaati);

            var conflict = FindConflict(context, sinavKayitDTO.Derslikler, sinavKayitDTO.DersBolumAkademikPersonelId,
                baslangicSaati, bitisSaati, sinavKayitDTO.SinavTarihi, excludeSinavId: null);
            if (conflict != null)
                return conflict; // transaction dispose edilince geri alınır

            SinavDetay sinavDetay = new SinavDetay
            {
                DersBolumAkademikPersonelId = sinavKayitDTO.DersBolumAkademikPersonelId,
                SinavTarihi = sinavKayitDTO.SinavTarihi,
                SinavBaslangicSaati = baslangicSaati,
                SinavBitisSaati = bitisSaati,
                CreatedDate = DateTime.Now,
                Status = true // ✅ Status alanını ekle
            };
            context.SinavDetay.Add(sinavDetay);
            context.SaveChanges();

            foreach (var derslik in sinavKayitDTO.Derslikler)
            {
                SinavDerslik sinavDerslik = new SinavDerslik
                {
                    SinavDetayId = sinavDetay.Id,
                    DerslikId = derslik.DerslikId,
                    GozetmenId = derslik.GozetmenId,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Status = true
                };
                context.SinavDerslik.Add(sinavDerslik);
            }

            context.SaveChanges();
            transaction.Commit();
            createdId = sinavDetay.Id;
            return null;
        }

        public SinavBildirimBilgisiDTO? GetBildirimBilgisi(int sinavDetayId)
        {
            using var context = new DuzceUniversiteContext();
            var sinav = (from s in context.SinavDetay
                         join dba in context.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals dba.Id
                         join d in context.Ders on dba.DersId equals d.Id
                         join ap in context.AkademikPersonel on dba.AkademikPersonelId equals ap.Id
                         where s.Id == sinavDetayId
                         select new { s.Id, d.Kod, d.Ad, s.SinavTarihi, s.SinavBaslangicSaati, s.SinavBitisSaati, ap.UserId })
                        .FirstOrDefault();
            if (sinav == null)
                return null;

            var derslikler = (from sd in context.SinavDerslik
                              join dl in context.Derslik on sd.DerslikId equals dl.Id
                              join ap in context.AkademikPersonel on sd.GozetmenId equals (int?)ap.Id into gozetmen
                              from g in gozetmen.DefaultIfEmpty()
                              where sd.SinavDetayId == sinavDetayId
                              select new { dl.Ad, GozetmenUserId = g == null ? (int?)null : g.UserId })
                             .ToList();

            var info = new SinavBildirimBilgisiDTO
            {
                SinavId = sinav.Id,
                DersKod = sinav.Kod,
                DersAd = sinav.Ad,
                SinavTarihi = sinav.SinavTarihi,
                BaslangicSaati = sinav.SinavBaslangicSaati,
                BitisSaati = sinav.SinavBitisSaati,
                Derslikler = derslikler.Select(x => x.Ad).ToList()
            };
            info.Gorevliler.Add(new SinavGorevliDTO { UserId = sinav.UserId, Gorev = "ogretim" });
            info.Gorevliler.AddRange(derslikler
                .Where(x => x.GozetmenUserId.HasValue)
                .Select(x => new SinavGorevliDTO { UserId = x.GozetmenUserId!.Value, Gorev = "gozetmen", Derslik = x.Ad }));
            return info;
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
                        throw;
                    }
                }
            }
        }
        /// <summary>
        /// Sınav yazma işlemlerini (ekleme/güncelleme) veritabanı seviyesinde sıraya sokar.
        /// Önceden çakışma kontrolü ile kayıt ayrı context'lerde yapıldığı için aynı anda gelen iki
        /// istek aynı derslik/gözetmen için çakışan sınav oluşturabiliyordu.
        /// SQL Server: sp_getapplock, transaction bitince (commit/rollback) otomatik bırakılır.
        /// MySQL: GET_LOCK oturuma bağlıdır; dönen nesne dispose edilince RELEASE_LOCK ile bırakılır.
        /// Transaction'dan sonra tanımlanmalıdır (önce kilit bırakılır, sonra transaction kapanır).
        /// </summary>
        private static IDisposable AcquireSinavPlanlamaLock(DuzceUniversiteContext context)
        {
            const string hata = "Sınav planlama kilidi alınamadı, lütfen tekrar deneyin.";

            if (DuzceUniversiteContext.Provider == DatabaseProvider.MySql)
            {
                var sonuc = context.Database
                    .SqlQueryRaw<long?>("SELECT GET_LOCK('SinavPlanlama', 15) AS Value")
                    .AsEnumerable()
                    .FirstOrDefault();
                if (sonuc != 1)
                    throw new InvalidOperationException(hata);
                return new KilitBirakici(() => context.Database.ExecuteSqlRaw("DO RELEASE_LOCK('SinavPlanlama')"));
            }

            context.Database.ExecuteSqlRaw(
                "DECLARE @result int; " +
                "EXEC @result = sp_getapplock @Resource = N'SinavPlanlama', @LockMode = N'Exclusive', " +
                "@LockOwner = N'Transaction', @LockTimeout = 15000; " +
                "IF @result < 0 THROW 50001, N'" + hata + "', 1;");
            return new KilitBirakici(null);
        }

        private sealed class KilitBirakici : IDisposable
        {
            private Action? _birak;
            public KilitBirakici(Action? birak) => _birak = birak;

            public void Dispose()
            {
                var birak = _birak;
                _birak = null;
                birak?.Invoke();
            }
        }

        /// <summary>
        /// Aynı gün ve kesişen saat aralığında; aynı derslikte, aynı gözetmenle veya dersin akademik
        /// personelinin başka bir dersiyle çakışan ilk sınavı döner.
        /// </summary>
        /// <remarks>
        /// Önceki ExistSinav metoduna DersBolumAkademikPersonel (DBAP) ID'si gönderilip
        /// AkademikPersonelId ile karşılaştırılıyordu; personel çakışması bu yüzden doğru tespit
        /// edilmiyordu. Artık DBAP kaydından gerçek AkademikPersonelId okunur. Güncellemede
        /// sınavın kendisi sorgudan hariç tutulur (önceden ilk eşleşme kendisi olursa diğer
        /// çakışmalar gözden kaçıyordu).
        /// </remarks>
        private static SinavDetay? FindConflict(
            DuzceUniversiteContext context,
            List<DerslikGozetmenDTO> derslikler,
            int dersBolumAkademikPersonelId,
            TimeOnly sinavBaslangicSaati,
            TimeOnly sinavBitisSaati,
            DateTime sinavTarihi,
            int? excludeSinavId)
        {
            var derslikIdleri = derslikler.Select(d => d.DerslikId).ToList();
            var gozetmenIdleri = derslikler.Where(d => d.GozetmenId.HasValue).Select(d => d.GozetmenId!.Value).ToList();
            var gun = sinavTarihi.Date;

            int? akademikPersonelId = context.DersBolumAkademikPersonel
                .Where(x => x.Id == dersBolumAkademikPersonelId)
                .Select(x => (int?)x.AkademikPersonelId)
                .FirstOrDefault();

            var result = from s in context.SinavDetay
                         join sd in context.SinavDerslik on s.Id equals sd.SinavDetayId into sd_join
                         from sd in sd_join.DefaultIfEmpty()
                         join dba in context.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals dba.Id into dba_join
                         from dba in dba_join.DefaultIfEmpty()
                         where s.SinavTarihi.Date == gun &&
                               (excludeSinavId == null || s.Id != excludeSinavId) &&
                               (
                                   // Derslik çakışma kontrolü
                                   derslikIdleri.Contains(sd.DerslikId) ||
                                   // Gözetmen çakışma kontrolü
                                   (sd.GozetmenId != null && gozetmenIdleri.Contains(sd.GozetmenId.Value)) ||
                                   // Akademik personel çakışma kontrolü
                                   (akademikPersonelId != null && dba.AkademikPersonelId == akademikPersonelId) ||
                                   // Gözetmen o saatte başka bir sınavın dersini veriyor
                                   gozetmenIdleri.Contains(dba.AkademikPersonelId) ||
                                   // Dersin öğretim elemanı o saatte başka bir sınavda gözetmen
                                   (akademikPersonelId != null && sd.GozetmenId == akademikPersonelId)
                               ) &&
                               // Saat aralıkları kesişiyor mu?
                               s.SinavBaslangicSaati < sinavBitisSaati &&
                               s.SinavBitisSaati > sinavBaslangicSaati
                         select s;

            return result.FirstOrDefault();
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
                                 GozetmenId = sd.GozetmenId ?? 0,
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
                                         sd => sd.GozetmenId ?? 0,
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
                                 GozetmenId = sd.GozetmenId ?? 0,
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
                                 sd => sd.GozetmenId ?? 0,
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
                                 GozetmenId = sd.GozetmenId ?? 0,
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
                                 sd => sd.GozetmenId ?? 0,
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
                                 GozetmenId = sd.GozetmenId ?? 0,
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
                                         sd => sd.GozetmenId ?? 0,
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
                                 GozetmenId = sd.GozetmenId ?? 0,
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
                                         sd => sd.GozetmenId ?? 0,
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
                                 GozetmenId = sd.GozetmenId ?? 0,
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
                                         sd => sd.GozetmenId ?? 0,
                                         ap => ap.Id,
                                         (sd, ap) => ap)
                                     .ToList()
                             };
                return result.ToList();
            }
        }

        /// <summary>
        /// Güncellemeyi, sınavın kendisi hariç tutularak yapılan çakışma kontrolüyle birlikte
        /// aynı transaction ve uygulama kilidi altında yapar. Çakışma varsa güncelleme yapılmaz ve
        /// çakışan sınav döner; yoksa null döner.
        /// </summary>
        public SinavDetay? UpdateIfNoConflict(SinavGuncelleDTO sinavGuncelleDTO)
        {
            using DuzceUniversiteContext context = new DuzceUniversiteContext();
            using var transaction = context.Database.BeginTransaction();

            using var planlamaKilidi = AcquireSinavPlanlamaLock(context);

            var conflict = FindConflict(context, sinavGuncelleDTO.Derslikler, sinavGuncelleDTO.DersBolumAkademikPersonelId,
                sinavGuncelleDTO.SinavBaslangicSaati, sinavGuncelleDTO.SinavBitisSaati, sinavGuncelleDTO.SinavTarihi,
                excludeSinavId: sinavGuncelleDTO.Id);
            if (conflict != null)
                return conflict;

            var sinavDetay = context.SinavDetay.Find(sinavGuncelleDTO.Id);
            if (sinavDetay == null)
                return null;

            sinavDetay.DersBolumAkademikPersonelId = sinavGuncelleDTO.DersBolumAkademikPersonelId;
            sinavDetay.SinavTarihi = sinavGuncelleDTO.SinavTarihi;
            sinavDetay.SinavBaslangicSaati = sinavGuncelleDTO.SinavBaslangicSaati;
            sinavDetay.SinavBitisSaati = sinavGuncelleDTO.SinavBitisSaati;
            sinavDetay.UpdatedDate = DateTime.Now;
            sinavDetay.Status = true; // ✅ Status alanını da güncelle

            var existingSinavDerslikler = context.SinavDerslik.Where(x => x.SinavDetayId == sinavGuncelleDTO.Id);
            context.SinavDerslik.RemoveRange(existingSinavDerslikler);

            foreach (var derslik in sinavGuncelleDTO.Derslikler)
            {
                SinavDerslik sinavDerslik = new SinavDerslik
                {
                    SinavDetayId = sinavDetay.Id,
                    DerslikId = derslik.DerslikId,
                    GozetmenId = derslik.GozetmenId,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Status = true
                };
                context.SinavDerslik.Add(sinavDerslik);
            }

            context.SaveChanges();
            transaction.Commit();
            return null;
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
                                 GozetmenId = sd.GozetmenId ?? 0,
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
                                         sd => sd.GozetmenId ?? 0,
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
                                 GozetmenId = sd.GozetmenId ?? 0,
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
                                         sd => sd.GozetmenId ?? 0,
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
                                 GozetmenId = sd.GozetmenId ?? 0,
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
                                         sd => sd.GozetmenId ?? 0,
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
                                        sd => sd.GozetmenId ?? 0,
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
                                        sd => sd.GozetmenId ?? 0,
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
                                        sd => sd.GozetmenId ?? 0,
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
                                        sd => sd.GozetmenId ?? 0,
                                        ap => ap.Id,
                                        (sd, ap) => ap)
                                    .ToList()
                             };

                // Distinct yerine GroupBy kullanarak duplicate'leri kaldır
                return result
                    .GroupBy(x => x.Id)
                    .Select(g => g.First())
                    .ToList();
            }
        }
    }
}
