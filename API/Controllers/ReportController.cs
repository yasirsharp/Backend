using Business.Abstract;
using Business.Constants;
using Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Rapor işlemleri controller'ı
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly API.Security.BolumScope _scope;

        public ReportController(IReportService reportService, API.Security.BolumScope scope)
        {
            _reportService = reportService;
            _scope = scope;
        }

        private static int[] ParseBolumIds(string? bolumIds) =>
            string.IsNullOrEmpty(bolumIds)
                ? Array.Empty<int>()
                : bolumIds.Split(',').Select(id => int.TryParse(id.Trim(), out var v) ? v : 0).Where(id => id > 0).ToArray();

        /// <summary>Akademik personel: yalnızca dersini verdiği ve gözetmen olduğu sınavlar</summary>
        private Func<Entity.DTOs.SinavDetayDTO, bool>? OwnTasksFilter()
        {
            if (!_scope.OwnTasksOnly)
                return null;
            var me = _scope.PersonelId;
            var myDbaps = _scope.MyDbapIds();
            return s => myDbaps.Contains(s.DersBolumAkademikPersonelId) || (me != null && (s.Derslikler ?? new()).Any(d => d.GozetmenId == me));
        }

        /// <summary>Bölüm görevlisi ve görevli personel yalnızca kendi bölümlerinin raporunu alır (istenmezse tüm kendi bölümleri)</summary>
        private int[] ScopeBolumIds(int[] requested)
        {
            if (!_scope.IsScoped || _scope.OwnTasksOnly)
                return _scope.OwnTasksOnly ? Array.Empty<int>() : requested;
            var own = _scope.Rules.BolumIds;
            var allowed = requested.Where(own.Contains).ToArray();
            return allowed.Length > 0 ? allowed : own.Count > 0 ? own.ToArray() : new[] { -1 };
        }

        /// <summary>
        /// Resmi sınav takvimi raporu oluştur (PDF)
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi (yyyy-MM-dd)</param>
        /// <param name="endDate">Bitiş tarihi (yyyy-MM-dd)</param>
        /// <param name="bolumIds">Bölüm ID'leri (opsiyonel, virgülle ayrılmış)</param>
        /// <returns>PDF dosyası</returns>
        [HttpGet("official-sinav-report")]
        public IActionResult GetOfficialSinavReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? bolumIds = null)
        {
            var bolumIdArray = ScopeBolumIds(ParseBolumIds(bolumIds));

            // Öğrenci sadece kendi bölümünün takvimini alabilir (istediği bölümden bağımsız)
            if (!User.IsInAnyRole(Roles.Personel))
            {
                if (!int.TryParse(User.FindFirst("BolumId")?.Value, out var ogrenciBolumId))
                    return Forbid();
                bolumIdArray = new[] { ogrenciBolumId };
            }

            var result = _reportService.GenerateOfficialSinavReport(startDate, endDate, bolumIdArray, User.IsInAnyRole(Roles.Personel) ? OwnTasksFilter() : null);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            // PDF olarak döndür
            return File(result.Data, "application/pdf", $"Sinav_Takvimi_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        /// <summary>
        /// Gözetmen atama raporu oluştur (PDF)
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi (yyyy-MM-dd)</param>
        /// <param name="endDate">Bitiş tarihi (yyyy-MM-dd)</param>
        /// <returns>PDF dosyası</returns>
        [HttpGet("gozetmen-assignment-report")]
        [Authorize(Roles = Roles.Personel)]
        public IActionResult GetGozetmenAssignmentReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? bolumIds = null)
        {
            var result = _reportService.GenerateGozetmenAssignmentReport(startDate, endDate, ScopeBolumIds(ParseBolumIds(bolumIds)), OwnTasksFilter());

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return File(result.Data, "application/pdf", $"Gozetmen_Atama_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        /// <summary>
        /// Derslik kullanım raporu oluştur (PDF)
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi (yyyy-MM-dd)</param>
        /// <param name="endDate">Bitiş tarihi (yyyy-MM-dd)</param>
        /// <returns>PDF dosyası</returns>
        [HttpGet("derslik-usage-report")]
        [Authorize(Roles = Roles.Personel)]
        public IActionResult GetDerslikUsageReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? bolumIds = null)
        {
            var result = _reportService.GenerateDerslikUsageReport(startDate, endDate, ScopeBolumIds(ParseBolumIds(bolumIds)), OwnTasksFilter());

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return File(result.Data, "application/pdf", $"Derslik_Kullanim_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        /// <summary>
        /// Rapor sistemi durumunu kontrol et
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetReportStatus()
        {
            return Ok(new
            {
                status = "active",
                message = "Rapor sistemi aktif",
                availableReports = new[]
                {
                    new { name = "Resmi Sınav Takvimi", endpoint = "/api/report/official-sinav-report" },
                    new { name = "Gözetmen Atama Raporu", endpoint = "/api/report/gozetmen-assignment-report" },
                    new { name = "Derslik Kullanım Raporu", endpoint = "/api/report/derslik-usage-report" }
                }
            });
        }
    }
}
