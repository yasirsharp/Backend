using Core.DataAccess;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    /// <summary>
    /// Akademik personel müsaitlik verilerine erişim için interface
    /// </summary>
    public interface IAkademikPersonelMusaitlikDal : IEntityRepository<AkademikPersonelMusaitlik>
    {
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
        /// (Sınav gözetmen ataması için kullanılır)
        /// </summary>
        Task<List<int>> GetMesgulPersonelIdlerAsync(
            DateTime tarih, 
            TimeSpan? baslangicSaati, 
            TimeSpan? bitisSaati);

        /// <summary>
        /// Belirli bir tarih ve saat aralığında müsait olan personelleri getirir
        /// (Sınav gözetmen ataması için kullanılır)
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
    }
}
