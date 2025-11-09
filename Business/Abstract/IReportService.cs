using Core.Utilites.Results;
using Core.Utilities.Results;
using System;

namespace Business.Abstract
{
    /// <summary>
    /// Rapor servisi - RDLC raporları için
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Sınav takvimi için resmi rapor oluştur (RDLC)
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        /// <param name="bolumIds">Bölüm ID'leri (opsiyonel, boşsa tüm bölümler)</param>
        /// <returns>PDF byte array</returns>
        IDataResult<byte[]> GenerateOfficialSinavReport(DateTime startDate, DateTime endDate, int[] bolumIds = null);

        /// <summary>
        /// Gözetmen atama raporu oluştur
        /// </summary>
        IDataResult<byte[]> GenerateGozetmenAssignmentReport(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Derslik kullanım raporu oluştur
        /// </summary>
        IDataResult<byte[]> GenerateDerslikUsageReport(DateTime startDate, DateTime endDate);
    }
}
