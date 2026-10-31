using Core.Utilities.Results;
using Entity.Concrete;
using Entity.DTOs;
using System.Collections.Generic;

namespace Business.Abstract
{
    public interface IBugReportService
    {
        IDataResult<int> Add(BugReport report);
        IDataResult<List<BugReportSummaryDto>> GetList(string? state);
        IDataResult<BugReportSummaryDto> GetById(int id);
        (byte[] Data, string ContentType)? GetScreenshot(int id);
        IDataResult<int> CountNew();
        IResult UpdateState(int id, BugReportStateDto dto);
        IResult Delete(int id);
    }
}
