using Core.Utilities.Results;
using Entity.Concrete;
using Entity.DTOs;
using System.Collections.Generic;

namespace Business.Abstract
{
    public interface IBugReportService
    {
        IDataResult<int> Add(BugReport report);
        /// <summary>
        /// Kendiliğinden yakalanan hata: aynı parmak izli açık kayıt varsa sayacı artırır, yoksa yeni kayıt açar.
        /// IsNew yalnızca yeni kayıtta true (e-posta bir kez gitsin).
        /// </summary>
        IDataResult<(int Id, bool IsNew)> RecordAutomatic(BugReport report);
        IDataResult<List<BugReportSummaryDto>> GetList(string? state);
        IDataResult<BugReportSummaryDto> GetById(int id);
        (byte[] Data, string ContentType)? GetScreenshot(int id);
        IDataResult<int> CountNew();
        IResult UpdateState(int id, BugReportStateDto dto);
        IResult Delete(int id);
    }
}
