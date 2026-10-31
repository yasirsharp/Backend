using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entity.Concrete;
using Entity.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfBugReportDal : EfEntityRepositoryBase<BugReport, DuzceUniversiteContext>, IBugReportDal
    {
        public List<BugReportSummaryDto> GetSummaries(string? state = null, int? id = null)
        {
            using var context = new DuzceUniversiteContext();
            var query = context.BugReports.AsQueryable();
            if (!string.IsNullOrEmpty(state))
                query = query.Where(x => x.State == state);
            if (id != null)
                query = query.Where(x => x.Id == id);
            return query
                .OrderByDescending(x => x.Id)
                .Select(x => new BugReportSummaryDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserName = x.UserName,
                    Roles = x.Roles,
                    Description = x.Description,
                    PageUrl = x.PageUrl,
                    AppVersion = x.AppVersion,
                    UserAgent = x.UserAgent,
                    Viewport = x.Viewport,
                    ClientLog = x.ClientLog,
                    HasScreenshot = x.Screenshot != null,
                    State = x.State,
                    AdminNote = x.AdminNote,
                    CreatedDate = x.CreatedDate,
                    ResolvedDate = x.ResolvedDate
                })
                .ToList();
        }

        public (byte[] Data, string ContentType)? GetScreenshot(int id)
        {
            using var context = new DuzceUniversiteContext();
            var row = context.BugReports
                .Where(x => x.Id == id && x.Screenshot != null)
                .Select(x => new { x.Screenshot, x.ScreenshotContentType })
                .FirstOrDefault();
            return row == null ? null : (row.Screenshot!, row.ScreenshotContentType ?? "image/jpeg");
        }

        public int CountByState(string state)
        {
            using var context = new DuzceUniversiteContext();
            return context.BugReports.Count(x => x.State == state);
        }
    }
}
