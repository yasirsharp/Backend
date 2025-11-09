using Business.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Rapor işlemleri controller'ı
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Resmi sınav takvimi raporu oluştur (PDF)
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi (yyyy-MM-dd)</param>
        /// <param name="endDate">Bitiş tarihi (yyyy-MM-dd)</param>
        /// <param name="bolumIds">Bölüm ID'leri (opsiyonel, virgülle ayrılmış)</param>
        /// <returns>PDF dosyası</returns>
        [HttpGet("official-sinav-report")]
        [Authorize]
        public IActionResult GetOfficialSinavReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? bolumIds = null)
        {
            // Bölüm ID'lerini parse et
            int[] bolumIdArray = null;
            if (!string.IsNullOrEmpty(bolumIds))
            {
                bolumIdArray = bolumIds.Split(',')
                    .Select(id => int.TryParse(id.Trim(), out int result) ? result : 0)
                    .Where(id => id > 0)
                    .ToArray();
            }

            var result = _reportService.GenerateOfficialSinavReport(startDate, endDate, bolumIdArray);

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
        [Authorize]
        public IActionResult GetGozetmenAssignmentReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var result = _reportService.GenerateGozetmenAssignmentReport(startDate, endDate);

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
        [Authorize]
        public IActionResult GetDerslikUsageReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var result = _reportService.GenerateDerslikUsageReport(startDate, endDate);

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
        [Authorize]
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
