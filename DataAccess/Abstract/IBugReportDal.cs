using Core.DataAccess;
using Entity.Concrete;
using Entity.DTOs;
using System.Collections.Generic;

namespace DataAccess.Abstract
{
    public interface IBugReportDal : IEntityRepository<BugReport>
    {
        /// <summary>Ekran görüntüsü yüklenmeden, yeniden eskiye</summary>
        List<BugReportSummaryDto> GetSummaries(string? state = null, int? id = null);
        (byte[] Data, string ContentType)? GetScreenshot(int id);
        int CountByState(string state);
    }
}
