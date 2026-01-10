using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Concrete
{
    /// <summary>
    /// Akademik personel müsaitlik yönetimi iş mantığı katmanı
    /// Sınav gözetmen ataması için müsaitlik kontrolü sağlar
    /// </summary>
    public class AkademikPersonelMusaitlikManager : IAkademikPersonelMusaitlikService
    {
        private readonly IAkademikPersonelMusaitlikDal _musaitlikDal;

        public AkademikPersonelMusaitlikManager(IAkademikPersonelMusaitlikDal musaitlikDal)
        {
            _musaitlikDal = musaitlikDal;
        }

        #region CRUD İşlemleri

        /// <summary>
        /// Yeni müsaitlik kaydı ekler
        /// Tekrar tipine göre TekrarGunu değerini otomatik ayarlar
        /// </summary>
        public async Task<IResult> AddAsync(AkademikPersonelMusaitlik musaitlik)
        {
            // Tekrar tipine göre TekrarGunu'nu otomatik ayarla
            SetTekrarGunu(musaitlik);

            // Audit alanlarını ayarla
            musaitlik.CreatedDate = DateTime.Now;
            musaitlik.Status = true;

            _musaitlikDal.Add(musaitlik);
            return new SuccessResult(Messages.MusaitlikAdded);
        }

        /// <summary>
        /// Müsaitlik kaydını günceller
        /// </summary>
        public async Task<IResult> UpdateAsync(AkademikPersonelMusaitlik musaitlik)
        {
            var existing = _musaitlikDal.Get(m => m.Id == musaitlik.Id);
            if (existing == null)
                return new ErrorResult(Messages.MusaitlikNotFound);

            // Tekrar tipine göre TekrarGunu'nu güncelle
            SetTekrarGunu(musaitlik);

            // Audit alanlarını güncelle
            musaitlik.CreatedDate = existing.CreatedDate;
            musaitlik.UpdatedDate = DateTime.Now;

            _musaitlikDal.Update(musaitlik);
            return new SuccessResult(Messages.MusaitlikUpdated);
        }

        /// <summary>
        /// Müsaitlik kaydını soft delete yapar
        /// </summary>
        public async Task<IResult> DeleteAsync(int id)
        {
            var musaitlik = _musaitlikDal.Get(m => m.Id == id);
            if (musaitlik == null)
                return new ErrorResult(Messages.MusaitlikNotFound);

            // Soft delete
            musaitlik.Status = false;
            musaitlik.UpdatedDate = DateTime.Now;

            _musaitlikDal.Update(musaitlik);
            return new SuccessResult(Messages.MusaitlikDeleted);
        }

        /// <summary>
        /// ID'ye göre müsaitlik kaydını getirir
        /// </summary>
        public IDataResult<AkademikPersonelMusaitlik> GetById(int id)
        {
            var musaitlik = _musaitlikDal.Get(m => m.Id == id && m.Status);
            if (musaitlik == null)
                return new ErrorDataResult<AkademikPersonelMusaitlik>(Messages.MusaitlikNotFound);

            return new SuccessDataResult<AkademikPersonelMusaitlik>(musaitlik);
        }

        #endregion

        #region Sorgulama İşlemleri

        /// <summary>
        /// Akademik personelin tüm aktif müsaitlik kayıtlarını getirir
        /// </summary>
        public async Task<IDataResult<List<AkademikPersonelMusaitlik>>> GetByPersonelIdAsync(int akademikPersonelId)
        {
            var kayitlar = await _musaitlikDal.GetByAkademikPersonelIdAsync(akademikPersonelId);
            return new SuccessDataResult<List<AkademikPersonelMusaitlik>>(kayitlar);
        }

        /// <summary>
        /// Akademik personelin belirli ay için müsaitlik takvimini getirir
        /// </summary>
        public async Task<IDataResult<List<AkademikPersonelMusaitlik>>> GetTakvimAsync(
            int akademikPersonelId,
            int yil,
            int ay)
        {
            // Ayın ilk ve son günü
            var ayBaslangic = new DateTime(yil, ay, 1);
            var ayBitis = ayBaslangic.AddMonths(1).AddDays(-1);

            var kayitlar = await _musaitlikDal.GetByDateRangeAsync(akademikPersonelId, ayBaslangic, ayBitis);
            return new SuccessDataResult<List<AkademikPersonelMusaitlik>>(kayitlar);
        }

        /// <summary>
        /// Belirli tarih aralığındaki müsaitlik kayıtlarını getirir
        /// </summary>
        public async Task<IDataResult<List<AkademikPersonelMusaitlik>>> GetByDateRangeAsync(
            int akademikPersonelId,
            DateTime baslangic,
            DateTime bitis)
        {
            var kayitlar = await _musaitlikDal.GetByDateRangeAsync(akademikPersonelId, baslangic, bitis);
            return new SuccessDataResult<List<AkademikPersonelMusaitlik>>(kayitlar);
        }

        #endregion

        #region Gözetmen Atama İşlemleri

        /// <summary>
        /// Belirli tarih ve saatte müsait olan personelleri getirir
        /// Sınav gözetmen ataması için kullanılır
        /// </summary>
        public async Task<IDataResult<List<AkademikPersonel>>> GetMusaitPersonellerAsync(
            DateTime tarih,
            TimeSpan? baslangicSaati,
            TimeSpan? bitisSaati)
        {
            var musaitPersoneller = await _musaitlikDal.GetMusaitPersonellerAsync(tarih, baslangicSaati, bitisSaati);
            return new SuccessDataResult<List<AkademikPersonel>>(musaitPersoneller);
        }

        /// <summary>
        /// Belirli tarih ve saatte meşgul olan personel ID'lerini getirir
        /// </summary>
        public async Task<IDataResult<List<int>>> GetMesgulPersonelIdlerAsync(
            DateTime tarih,
            TimeSpan? baslangicSaati,
            TimeSpan? bitisSaati)
        {
            var mesgulIdler = await _musaitlikDal.GetMesgulPersonelIdlerAsync(tarih, baslangicSaati, bitisSaati);
            return new SuccessDataResult<List<int>>(mesgulIdler);
        }

        /// <summary>
        /// Personelin belirli tarih ve saatte meşgul olup olmadığını kontrol eder
        /// </summary>
        public async Task<IDataResult<bool>> IsMesgulAsync(
            int akademikPersonelId,
            DateTime tarih,
            TimeSpan? baslangicSaati,
            TimeSpan? bitisSaati)
        {
            var isMesgul = await _musaitlikDal.IsMesgulAsync(
                akademikPersonelId, tarih, baslangicSaati, bitisSaati);

            var message = isMesgul ? Messages.PersonelMesgul : Messages.PersonelMusait;
            return new SuccessDataResult<bool>(isMesgul, message);
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Tekrar tipine göre TekrarGunu değerini otomatik ayarlar
        /// </summary>
        private void SetTekrarGunu(AkademikPersonelMusaitlik musaitlik)
        {
            switch (musaitlik.TekrarTipi)
            {
                case TekrarTipiEnum.Tekil:
                    // Tekil kayıtlarda TekrarGunu kullanılmaz
                    musaitlik.TekrarGunu = null;
                    break;

                case TekrarTipiEnum.Haftalik:
                    // Haftalık tekrar: Başlangıç tarihinin haftanın günü
                    // DayOfWeek: 0=Pazar, 1=Pazartesi, ..., 6=Cumartesi
                    musaitlik.TekrarGunu = (int)musaitlik.BaslangicTarihi.DayOfWeek;
                    break;

                case TekrarTipiEnum.Aylik:
                    // Aylık tekrar: Başlangıç tarihinin ayın günü
                    musaitlik.TekrarGunu = musaitlik.BaslangicTarihi.Day;
                    break;
            }
        }

        #endregion
    }
}
