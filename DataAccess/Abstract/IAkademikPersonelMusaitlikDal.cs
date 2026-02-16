using Core.DataAccess;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    /// <summary>
    /// Akademik personel müsaitlik verilerine erişim için interface
    /// </summary>
    public interface IAkademikPersonelMusaitlikDal : IEntityRepository<AkademikPersonelMusaitlik>
    {
        #region Async CRUD

        /// <summary>
        /// Async kayıt ekleme
        /// </summary>
        Task AddAsync(AkademikPersonelMusaitlik entity);

        /// <summary>
        /// Async kayıt güncelleme
        /// </summary>
        Task UpdateAsync(AkademikPersonelMusaitlik entity);

        /// <summary>
        /// Filtreye göre async kayıt getirme
        /// </summary>
        Task<AkademikPersonelMusaitlik?> GetAsync(Expression<Func<AkademikPersonelMusaitlik, bool>> filter);

        /// <summary>
        /// Toplu silme (soft delete)
        /// </summary>
        Task<int> DeleteBatchAsync(List<int> ids);

        #endregion

        #region Çakışma Kontrolü

        /// <summary>
        /// Belirli personel ve zaman diliminde çakışan kayıt var mı kontrol eder
        /// </summary>
        Task<bool> HasOverlapAsync(
            int akademikPersonelId,
            DateTime baslangicTarihi,
            DateTime? bitisTarihi,
            TimeSpan? baslangicSaati,
            TimeSpan? bitisSaati,
            TekrarTipiEnum tekrarTipi,
            int? excludeId = null);

        #endregion

        #region Sorgulama

        /// <summary>
        /// Belirli bir akademik personelin tüm müsaitlik kayıtlarını getirir
        /// </summary>
        Task<List<AkademikPersonelMusaitlik>> GetByAkademikPersonelIdAsync(int akademikPersonelId);

        /// <summary>
        /// Belirli bir tarih aralığındaki müsaitlik kayıtlarını getirir
        /// </summary>
        Task<List<AkademikPersonelMusaitlik>> GetByDateRangeAsync(
            int akademikPersonelId, 
            DateTime baslangic, 
            DateTime bitis);

        /// <summary>
        /// Belirli bir tarih ve saat aralığında meşgul olan personellerin ID'lerini getirir
        /// </summary>
        Task<List<int>> GetMesgulPersonelIdlerAsync(
            DateTime tarih, 
            TimeSpan? baslangicSaati, 
            TimeSpan? bitisSaati);

        /// <summary>
        /// Belirli bir tarih ve saat aralığında müsait olan personelleri getirir
        /// </summary>
        Task<List<AkademikPersonel>> GetMusaitPersonellerAsync(
            DateTime tarih, 
            TimeSpan? baslangicSaati, 
            TimeSpan? bitisSaati);

        /// <summary>
        /// Belirli bir personelin belirli bir tarihte meşgul olup olmadığını kontrol eder
        /// </summary>
        Task<bool> IsMesgulAsync(
            int akademikPersonelId, 
            DateTime tarih,
            TimeSpan? baslangicSaati,
            TimeSpan? bitisSaati);

        #endregion
    }
}
