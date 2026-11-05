using Business.Abstract;
using Business.Helpers;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entity.Concrete;
using Entity.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Concrete
{
    public class BugReportManager : IBugReportService
    {
        private const string NotFound = "Hata bildirimi bulunamadı.";
        private readonly IBugReportDal _bugReportDal;

        public BugReportManager(IBugReportDal bugReportDal)
        {
            _bugReportDal = bugReportDal;
        }

        public IDataResult<int> Add(BugReport report)
        {
            report.Description = BugReportRules.Clip(report.Description, BugReportRules.MaxDescriptionChars);
            if (report.Description.Length == 0)
                return new ErrorDataResult<int>("Ne olduğunu kısaca yazın.");

            var screenshotError = BugReportRules.ValidateScreenshot(report.Screenshot);
            if (screenshotError != null)
                return new ErrorDataResult<int>(screenshotError);
            if (report.Screenshot is { Length: 0 })
                report.Screenshot = null;
            report.ScreenshotContentType = report.Screenshot == null ? null : BugReportRules.DetectImageType(report.Screenshot);

            Normalize(report);
            report.Source = BugReportSources.Kullanici;
            report.Fingerprint = null;
            report.OccurrenceCount = 1;
            report.LastSeenDate = report.CreatedDate = DateTime.Now;
            report.AdminNote = null;

            _bugReportDal.Add(report);
            return new SuccessDataResult<int>(report.Id, "Hata bildiriminiz alındı. Teşekkürler!");
        }

        public IDataResult<(int Id, bool IsNew)> RecordAutomatic(BugReport report)
        {
            report.Description = BugReportRules.Clip(report.Description, BugReportRules.MaxDescriptionChars);
            if (report.Description.Length == 0 || string.IsNullOrEmpty(report.Fingerprint))
                return new ErrorDataResult<(int, bool)>("Eksik hata bilgisi.");
            report.Screenshot = null;
            report.ScreenshotContentType = null;
            Normalize(report);

            var now = DateTime.Now;
            var open = _bugReportDal.Get(x => x.Fingerprint == report.Fingerprint && x.State != BugReportStates.Cozuldu);
            if (open != null)
            {
                open.OccurrenceCount++;
                open.LastSeenDate = now;
                open.UpdatedDate = now;
                _bugReportDal.Update(open);
                return new SuccessDataResult<(int, bool)>((open.Id, false));
            }

            var resolved = _bugReportDal.GetAll(x => x.Fingerprint == report.Fingerprint && x.State == BugReportStates.Cozuldu)
                .OrderByDescending(x => x.Id).FirstOrDefault();
            report.AdminNote = resolved == null ? null : $"Daha önce çözüldü olarak kapatılan hata (#{resolved.Id}) tekrar etti.";
            report.OccurrenceCount = 1;
            report.LastSeenDate = report.CreatedDate = now;
            _bugReportDal.Add(report);
            return new SuccessDataResult<(int, bool)>((report.Id, true));
        }

        /// <summary>Alanları kırpar ve yeni kaydın durumunu sıfırlar (istemciye güvenilmez)</summary>
        private static void Normalize(BugReport report)
        {
            report.UserName = BugReportRules.Clip(report.UserName, 200);
            report.Roles = BugReportRules.Clip(report.Roles, 300);
            report.PageUrl = BugReportRules.Clip(report.PageUrl, 500);
            report.AppVersion = BugReportRules.Clip(report.AppVersion, 100);
            report.UserAgent = BugReportRules.Clip(report.UserAgent, 500);
            report.Viewport = BugReportRules.Clip(report.Viewport, 50);
            report.ClientLog = string.IsNullOrWhiteSpace(report.ClientLog)
                ? null
                : BugReportRules.Clip(report.ClientLog, BugReportRules.MaxClientLogChars);
            report.State = BugReportStates.Yeni;
            report.ResolvedDate = null;
            report.Status = true;
        }

        public IDataResult<List<BugReportSummaryDto>> GetList(string? state)
        {
            if (!string.IsNullOrEmpty(state) && !BugReportStates.All.Contains(state))
                return new ErrorDataResult<List<BugReportSummaryDto>>("Geçersiz durum.");
            var list = _bugReportDal.GetSummaries(state);
            return new SuccessDataResult<List<BugReportSummaryDto>>(list, $"{list.Count} hata bildirimi bulundu.");
        }

        public IDataResult<BugReportSummaryDto> GetById(int id)
        {
            var item = _bugReportDal.GetSummaries(id: id).FirstOrDefault();
            return item == null
                ? new ErrorDataResult<BugReportSummaryDto>(NotFound)
                : new SuccessDataResult<BugReportSummaryDto>(item);
        }

        public (byte[] Data, string ContentType)? GetScreenshot(int id) => _bugReportDal.GetScreenshot(id);

        public IDataResult<int> CountNew() =>
            new SuccessDataResult<int>(_bugReportDal.CountByState(BugReportStates.Yeni));

        public IResult UpdateState(int id, BugReportStateDto dto)
        {
            if (!BugReportStates.All.Contains(dto.State))
                return new ErrorResult("Geçersiz durum.");
            var report = _bugReportDal.Get(x => x.Id == id);
            if (report == null)
                return new ErrorResult(NotFound);

            report.State = dto.State;
            report.AdminNote = string.IsNullOrWhiteSpace(dto.AdminNote) ? null : BugReportRules.Clip(dto.AdminNote, 2000);
            report.ResolvedDate = dto.State == BugReportStates.Cozuldu ? report.ResolvedDate ?? DateTime.Now : null;
            report.UpdatedDate = DateTime.Now;
            _bugReportDal.Update(report);
            return new SuccessResult("Hata bildirimi güncellendi.");
        }

        public IResult Delete(int id)
        {
            var report = _bugReportDal.Get(x => x.Id == id);
            if (report == null)
                return new ErrorResult(NotFound);
            _bugReportDal.Delete(report);
            return new SuccessResult("Hata bildirimi silindi.");
        }
    }
}
