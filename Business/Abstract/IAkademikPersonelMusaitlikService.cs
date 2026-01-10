using Core.Utilities.Results;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Abstract
{
    /// <summary>
    /// Akademik personel müsaitlik yönetimi için servis interface'i
    /// </summary>
    public interface IAkademikPersonelMusaitlikService
    {
        #region CRUD İşlemleri

        /// <summary>
        /// Yeni müsaitlik kaydı ekler
        /// </summary>
        Task<IResult> AddAsync(AkademikPersonelMusaitlik musaitlik);

        /// <summary>
        /// Müsaitlik kaydını günceller
        /// </summary>
        Task<IResult> UpdateAsync(AkademikPersonelMusaitlik musaitlik);

        /// <summary>
        /// Müsaitlik kaydını siler (soft delete)
        /// </summary>
        Task<IResult> DeleteAsync(int id);

        /// <summary>
        /// ID'ye göre müsaitlik kaydını getirir
        /// </summary>
        IDataResult<AkademikPersonelMusaitlik> GetById(int id);

        #endregion

        #region Sorgulama İşlemleri

        /// <summary>
        /// Akademik personelin tüm müsaitlik kayıtlarını getirir
        /// </summary>
        Task<IDataResult<List<AkademikPersonelMusaitlik>>> GetByPersonelIdAsync(int akademikPersonelId);

        /// <summary>
        /// Akademik personelin belirli ay için müsaitlik takvimini getirir
        /// </summary>
        Task<IDataResult<List<AkademikPersonelMusaitlik>>> GetTakvimAsync(
            int akademikPersonelId, 
            int yil, 
            int ay);

        /// <summary>
        /// Belirli tarih aralığındaki müsaitlik kayıtlarını getirir
        /// </summary>
        Task<IDataResult<List<AkademikPersonelMusaitlik>>> GetByDateRangeAsync(
            int akademikPersonelId,
            DateTime baslangic,
            DateTime bitis);

        #endregion

        #region Gözetmen Atama İşlemleri

        /// <summary>
        /// Belirli tarih ve saatte müsait olan personelleri getirir
        /// (Sınav gözetmen ataması için)
        /// </summary>
        Task<IDataResult<List<AkademikPersonel>>> GetMusaitPersonellerAsync(
            DateTime tarih,
            TimeSpan? baslangicSaati,
            TimeSpan? bitisSaati);

        /// <summary>
        /// Belirli tarih ve saatte meşgul olan personel ID'lerini getirir
        /// </summary>
        Task<IDataResult<List<int>>> GetMesgulPersonelIdlerAsync(
            DateTime tarih,
            TimeSpan? baslangicSaati,
            TimeSpan? bitisSaati);

        /// <summary>
        /// Personelin belirli tarih ve saatte meşgul olup olmadığını kontrol eder
        /// </summary>
        Task<IDataResult<bool>> IsMesgulAsync(
            int akademikPersonelId,
            DateTime tarih,
            TimeSpan? baslangicSaati,
            TimeSpan? bitisSaati);

        #endregion
    }
}
