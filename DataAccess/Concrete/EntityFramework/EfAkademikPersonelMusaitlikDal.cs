using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entity.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

            // Tarih aralığında geçerli olan kayıtları filtrele
            return tumKayitlar
                .Where(m => KayitTarihAraligindaMi(m, baslangic, bitis))
                .ToList();
        }

        /// <summary>
        /// Belirli bir tarih ve saat aralığında meşgul olan personellerin ID'lerini getirir
        /// </summary>
        public async Task<List<int>> GetMesgulPersonelIdlerAsync(
            DateTime tarih,
            TimeSpan? baslangicSaati,
            TimeSpan? bitisSaati)
        {
            var tumKayitlar = await _context.AkademikPersonelMusaitlik
                .Where(m => m.Status)
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
            // Meşgul personel ID'lerini al
            var mesgulPersonelIdler = await GetMesgulPersonelIdlerAsync(tarih, baslangicSaati, bitisSaati);

            // Meşgul olmayan (müsait) personelleri getir
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
                .Where(m => m.AkademikPersonelId == akademikPersonelId && m.Status)
                .ToListAsync();

            return kayitlar.Any(m => KayitTarihVeSaatteGecerliMi(m, tarih, baslangicSaati, bitisSaati));
        }

        #region Private Helper Methods

        /// <summary>
        /// Kayıt belirtilen tarih aralığında geçerli mi kontrol eder
        /// Tekrarlı kayıtları da hesaba katar
        /// </summary>
        private bool KayitTarihAraligindaMi(
            AkademikPersonelMusaitlik kayit,
            DateTime baslangic,
            DateTime bitis)
        {
            switch (kayit.TekrarTipi)
            {
                case TekrarTipiEnum.Tekil:
                    // Tekil kayıt: Başlangıç tarihi aralıkta mı?
                    return kayit.BaslangicTarihi.Date >= baslangic.Date &&
                           kayit.BaslangicTarihi.Date <= bitis.Date;

                case TekrarTipiEnum.Haftalik:
                    // Haftalık: Kayıt aralığı ile sorgulanan aralık kesişiyor mu?
                    var kayitBaslangic = kayit.BaslangicTarihi.Date;
                    var kayitBitis = kayit.BitisTarihi?.Date ?? DateTime.MaxValue.Date;
                    return kayitBaslangic <= bitis.Date && kayitBitis >= baslangic.Date;

                case TekrarTipiEnum.Aylik:
                    // Aylık: Kayıt aralığı ile sorgulanan aralık kesişiyor mu?
                    var aylikBaslangic = kayit.BaslangicTarihi.Date;
                    var aylikBitis = kayit.BitisTarihi?.Date ?? DateTime.MaxValue.Date;
                    return aylikBaslangic <= bitis.Date && aylikBitis >= baslangic.Date;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Kayıt belirtilen tarih ve saatte geçerli mi kontrol eder
        /// Tekrarlı kayıtları ve saat çakışmasını hesaba katar
        /// </summary>
        private bool KayitTarihVeSaatteGecerliMi(
            AkademikPersonelMusaitlik kayit,
            DateTime tarih,
            TimeSpan? sorguBaslangicSaati,
            TimeSpan? sorguBitisSaati)
        {
            // Önce tarih kontrolü
            if (!TarihGecerliMi(kayit, tarih))
                return false;

            // Sonra saat çakışması kontrolü
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
                    // Tekil: Sadece başlangıç tarihinde geçerli
                    return kayitBaslangic == tarihDate;

                case TekrarTipiEnum.Haftalik:
                    // Haftalık: Tarih aralığında ve aynı haftanın günü
                    if (tarihDate < kayitBaslangic || tarihDate > kayitBitis)
                        return false;
                    return kayit.TekrarGunu.HasValue && 
                           (int)tarih.DayOfWeek == kayit.TekrarGunu.Value;

                case TekrarTipiEnum.Aylik:
                    // Aylık: Tarih aralığında ve ayın aynı günü
                    if (tarihDate < kayitBaslangic || tarihDate > kayitBitis)
                        return false;
                    return kayit.TekrarGunu.HasValue && 
                           tarih.Day == kayit.TekrarGunu.Value;

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
            var kayitBaslangic = kayit.BaslangicSaati.Value;
            var kayitBitis = kayit.BitisSaati.Value;
            var sorguBaslangicSaat = sorguBaslangic.Value;
            var sorguBitisSaat = sorguBitis.Value;

            // İki aralık çakışıyor mu? (A.Start < B.End && B.Start < A.End)
            return kayitBaslangic < sorguBitisSaat && sorguBaslangicSaat < kayitBitis;
        }

        #endregion
    }
}
