using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entity.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    /// <summary>
    /// Akademik personel müsaitlik verilerine Entity Framework ile erişim sağlar
    /// </summary>
    public class EfAkademikPersonelMusaitlikDal
        : EfEntityRepositoryBase<AkademikPersonelMusaitlik, DuzceUniversiteContext>,
          IAkademikPersonelMusaitlikDal
    {
        private readonly DuzceUniversiteContext _context;

        public EfAkademikPersonelMusaitlikDal(DuzceUniversiteContext context)
        {
            _context = context;
        }

        #region Async CRUD

        /// <summary>
        /// Async kayıt ekleme
        /// </summary>
        public async Task AddAsync(AkademikPersonelMusaitlik entity)
        {
            await _context.AkademikPersonelMusaitlik.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Async kayıt güncelleme
        /// </summary>
        public async Task UpdateAsync(AkademikPersonelMusaitlik entity)
        {
            entity.UpdatedDate = DateTime.Now;
            _context.AkademikPersonelMusaitlik.Update(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Filtreye göre async kayıt getirme
        /// </summary>
        public async Task<AkademikPersonelMusaitlik?> GetAsync(
            Expression<Func<AkademikPersonelMusaitlik, bool>> filter)
        {
            return await _context.AkademikPersonelMusaitlik.FirstOrDefaultAsync(filter);
        }

        /// <summary>
        /// Toplu silme (soft delete)
        /// </summary>
        public async Task<int> DeleteBatchAsync(List<int> ids)
        {
            var kayitlar = await _context.AkademikPersonelMusaitlik
                .Where(m => ids.Contains(m.Id) && m.Status)
                .ToListAsync();

            foreach (var kayit in kayitlar)
            {
                kayit.Status = false;
                kayit.UpdatedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return kayitlar.Count;
        }

        #endregion

        #region Çakışma Kontrolü

        /// <summary>
        /// Belirli personel ve zaman diliminde çakışan kayıt var mı kontrol eder
        /// Aynı gün ve saat aralığında aynı personele ait başka kayıt varsa true döner
        /// </summary>
        public async Task<bool> HasOverlapAsync(
            int akademikPersonelId,
            DateTime baslangicTarihi,
            DateTime? bitisTarihi,
            TimeSpan? baslangicSaati,
            TimeSpan? bitisSaati,
            TekrarTipiEnum tekrarTipi,
            int? excludeId = null)
        {
            var kayitlar = await _context.AkademikPersonelMusaitlik
                .Where(m => m.AkademikPersonelId == akademikPersonelId
                         && m.Status
                         && (!excludeId.HasValue || m.Id != excludeId.Value))
                .ToListAsync();

            // Yeni kaydın haftanın günü
            var yeniTekrarGunu = tekrarTipi switch
            {
                TekrarTipiEnum.Haftalik => (int)baslangicTarihi.DayOfWeek,
                TekrarTipiEnum.Aylik => baslangicTarihi.Day,
                _ => (int?)null
            };

            return kayitlar.Any(m =>
            {
                // Haftalık tekrar için: aynı haftanın günü mü kontrol et
                if (tekrarTipi == TekrarTipiEnum.Haftalik && m.TekrarTipi == TekrarTipiEnum.Haftalik)
                {
                    if (m.TekrarGunu != yeniTekrarGunu) return false;
                    return SaatCakisiyorMu(m, baslangicSaati, bitisSaati);
                }

                // Tekil tekrar: aynı tarih mi kontrol et
                if (tekrarTipi == TekrarTipiEnum.Tekil && m.TekrarTipi == TekrarTipiEnum.Tekil)
                {
                    if (m.BaslangicTarihi.Date != baslangicTarihi.Date) return false;
                    return SaatCakisiyorMu(m, baslangicSaati, bitisSaati);
                }

                // Aylık tekrar: aynı ayın günü mü kontrol et
                if (tekrarTipi == TekrarTipiEnum.Aylik && m.TekrarTipi == TekrarTipiEnum.Aylik)
                {
                    if (m.TekrarGunu != yeniTekrarGunu) return false;
                    return SaatCakisiyorMu(m, baslangicSaati, bitisSaati);
                }

                return false;
            });
        }

        #endregion

        #region Sorgulama

        /// <summary>
        /// Belirli bir akademik personelin tüm aktif müsaitlik kayıtlarını getirir
        /// </summary>
        public async Task<List<AkademikPersonelMusaitlik>> GetByAkademikPersonelIdAsync(int akademikPersonelId)
        {
            return await _context.AkademikPersonelMusaitlik
                .Where(m => m.AkademikPersonelId == akademikPersonelId && m.Status)
                .OrderByDescending(m => m.BaslangicTarihi)
                .ToListAsync();
        }

        /// <summary>
        /// Belirli bir tarih aralığındaki müsaitlik kayıtlarını getirir
        /// Tekrarlı kayıtları da hesaba katar
        /// </summary>
        public async Task<List<AkademikPersonelMusaitlik>> GetByDateRangeAsync(
            int akademikPersonelId,
            DateTime baslangic,
            DateTime bitis)
        {
            var tumKayitlar = await _context.AkademikPersonelMusaitlik
                .Where(m => m.AkademikPersonelId == akademikPersonelId && m.Status)
                .ToListAsync();

            return tumKayitlar
                .Where(m => KayitTarihAraligindaMi(m, baslangic, bitis))
                .ToList();
        }

        /// <summary>
        /// Belirli bir tarih ve saat aralığında meşgul olan personellerin ID'lerini getirir
        /// IsMusait = false olan kayıtları kontrol eder
        /// </summary>
        public async Task<List<int>> GetMesgulPersonelIdlerAsync(
            DateTime tarih,
            TimeSpan? baslangicSaati,
            TimeSpan? bitisSaati)
        {
            var tumKayitlar = await _context.AkademikPersonelMusaitlik
                .Where(m => m.Status && !m.IsMusait) // Sadece meşgul kayıtları kontrol et
                .ToListAsync();

            return tumKayitlar
                .Where(m => KayitTarihVeSaatteGecerliMi(m, tarih, baslangicSaati, bitisSaati))
                .Select(m => m.AkademikPersonelId)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Belirli bir tarih ve saat aralığında müsait olan personelleri getirir
        /// </summary>
        public async Task<List<AkademikPersonel>> GetMusaitPersonellerAsync(
            DateTime tarih,
            TimeSpan? baslangicSaati,
            TimeSpan? bitisSaati)
        {
            var mesgulPersonelIdler = await GetMesgulPersonelIdlerAsync(tarih, baslangicSaati, bitisSaati);

            return await _context.AkademikPersonel
                .Where(p => p.Status && !mesgulPersonelIdler.Contains(p.Id))
                .ToListAsync();
        }

        /// <summary>
        /// Belirli bir personelin belirli bir tarihte meşgul olup olmadığını kontrol eder
        /// </summary>
        public async Task<bool> IsMesgulAsync(
            int akademikPersonelId,
            DateTime tarih,
            TimeSpan? baslangicSaati,
            TimeSpan? bitisSaati)
        {
            var kayitlar = await _context.AkademikPersonelMusaitlik
                .Where(m => m.AkademikPersonelId == akademikPersonelId 
                         && m.Status 
                         && !m.IsMusait) // Sadece meşgul kayıtları kontrol et
                .ToListAsync();

            return kayitlar.Any(m => KayitTarihVeSaatteGecerliMi(m, tarih, baslangicSaati, bitisSaati));
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Kayıt belirtilen tarih aralığında geçerli mi kontrol eder
        /// </summary>
        private bool KayitTarihAraligindaMi(
            AkademikPersonelMusaitlik kayit,
            DateTime baslangic,
            DateTime bitis)
        {
            switch (kayit.TekrarTipi)
            {
                case TekrarTipiEnum.Tekil:
                    // Tekil kayıt: Başlangıç veya bitiş tarihi aralıkta mı?
                    var tekilBitis = kayit.BitisTarihi?.Date ?? kayit.BaslangicTarihi.Date;
                    return kayit.BaslangicTarihi.Date <= bitis.Date &&
                           tekilBitis >= baslangic.Date;

                case TekrarTipiEnum.Haftalik:
                case TekrarTipiEnum.Aylik:
                    var kayitBaslangic = kayit.BaslangicTarihi.Date;
                    var kayitBitis = kayit.BitisTarihi?.Date ?? DateTime.MaxValue.Date;
                    return kayitBaslangic <= bitis.Date && kayitBitis >= baslangic.Date;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Kayıt belirtilen tarih ve saatte geçerli mi kontrol eder
        /// </summary>
        private bool KayitTarihVeSaatteGecerliMi(
            AkademikPersonelMusaitlik kayit,
            DateTime tarih,
            TimeSpan? sorguBaslangicSaati,
            TimeSpan? sorguBitisSaati)
        {
            if (!TarihGecerliMi(kayit, tarih))
                return false;

            return SaatCakisiyorMu(kayit, sorguBaslangicSaati, sorguBitisSaati);
        }

        /// <summary>
        /// Kayıt belirtilen tarihte geçerli mi? (Tekrar tipine göre)
        /// </summary>
        private bool TarihGecerliMi(AkademikPersonelMusaitlik kayit, DateTime tarih)
        {
            var tarihDate = tarih.Date;
            var kayitBaslangic = kayit.BaslangicTarihi.Date;
            var kayitBitis = kayit.BitisTarihi?.Date ?? DateTime.MaxValue.Date;

            switch (kayit.TekrarTipi)
            {
                case TekrarTipiEnum.Tekil:
                    // Tekil: Tarih aralığında mı?
                    var tekilBitis = kayit.BitisTarihi?.Date ?? kayit.BaslangicTarihi.Date;
                    return tarihDate >= kayitBaslangic && tarihDate <= tekilBitis;

                case TekrarTipiEnum.Haftalik:
                    if (tarihDate < kayitBaslangic || tarihDate > kayitBitis)
                        return false;
                    return kayit.TekrarGunu.HasValue && 
                           (int)tarih.DayOfWeek == kayit.TekrarGunu.Value;

                case TekrarTipiEnum.Aylik:
                    if (tarihDate < kayitBaslangic || tarihDate > kayitBitis)
                        return false;
                    // Aylık tekrar: ayın günü eşleşiyorsa veya
                    // son gün mantığı (örn: 31'i olan ayda 31, olmayanlarda ayın son günü)
                    if (!kayit.TekrarGunu.HasValue) return false;
                    var ayinSonGunu = DateTime.DaysInMonth(tarih.Year, tarih.Month);
                    var hedefGun = Math.Min(kayit.TekrarGunu.Value, ayinSonGunu);
                    return tarih.Day == hedefGun;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Saat aralıkları çakışıyor mu kontrol eder
        /// Null saat = Tüm gün anlamına gelir
        /// </summary>
        private bool SaatCakisiyorMu(
            AkademikPersonelMusaitlik kayit,
            TimeSpan? sorguBaslangic,
            TimeSpan? sorguBitis)
        {
            // Kayıt tüm gün ise her zaman çakışır
            if (!kayit.BaslangicSaati.HasValue || !kayit.BitisSaati.HasValue)
                return true;

            // Sorgu tüm gün ise her zaman çakışır
            if (!sorguBaslangic.HasValue || !sorguBitis.HasValue)
                return true;

            // Her iki tarafta da saat var, çakışma kontrolü yap
            // İki aralık çakışıyor mu? (A.Start < B.End && B.Start < A.End)
            return kayit.BaslangicSaati.Value < sorguBitis.Value 
                && sorguBaslangic.Value < kayit.BitisSaati.Value;
        }

        #endregion
    }
}
