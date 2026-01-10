using Microsoft.AspNetCore.Mvc;
using Business.Abstract;
using Entity.Concrete;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    /// <summary>
    /// Akademik personel müsaitlik yönetimi API endpoint'leri
    /// Sınav gözetmen ataması için müsaitlik kontrolü sağlar
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AkademikPersonelMusaitlikController : ControllerBase
    {
        private readonly IAkademikPersonelMusaitlikService _musaitlikService;

        public AkademikPersonelMusaitlikController(IAkademikPersonelMusaitlikService musaitlikService)
        {
            _musaitlikService = musaitlikService;
        }

        #region CRUD Endpoint'leri

        /// <summary>
        /// Yeni müsaitlik kaydı ekler
        /// </summary>
        /// <param name="musaitlik">Müsaitlik bilgileri</param>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AkademikPersonelMusaitlik musaitlik)
        {
            var result = await _musaitlikService.AddAsync(musaitlik);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        /// <summary>
        /// Müsaitlik kaydını günceller
        /// </summary>
        /// <param name="musaitlik">Güncellenecek müsaitlik bilgileri</param>
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] AkademikPersonelMusaitlik musaitlik)
        {
            var result = await _musaitlikService.UpdateAsync(musaitlik);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Müsaitlik kaydını siler (soft delete)
        /// </summary>
        /// <param name="id">Silinecek kayıt ID'si</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _musaitlikService.DeleteAsync(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// ID'ye göre müsaitlik kaydını getirir
        /// </summary>
        /// <param name="id">Kayıt ID'si</param>
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var result = _musaitlikService.GetById(id);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        #endregion

        #region Personel Müsaitlik Sorgulama

        /// <summary>
        /// Akademik personelin tüm müsaitlik kayıtlarını getirir
        /// </summary>
        /// <param name="personelId">Akademik personel ID'si</param>
        [HttpGet("personel/{personelId}")]
        public async Task<IActionResult> GetByPersonelId(int personelId)
        {
            var result = await _musaitlikService.GetByPersonelIdAsync(personelId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Akademik personelin belirli ay için müsaitlik takvimini getirir
        /// Frontend takvim komponenti için kullanılır
        /// </summary>
        /// <param name="personelId">Akademik personel ID'si</param>
        /// <param name="yil">Yıl (örn: 2026)</param>
        /// <param name="ay">Ay (1-12)</param>
        [HttpGet("takvim/{personelId}/{yil}/{ay}")]
        public async Task<IActionResult> GetTakvim(int personelId, int yil, int ay)
        {
            if (ay < 1 || ay > 12)
                return BadRequest(new { Success = false, Message = "Ay 1-12 arasında olmalıdır." });

            var result = await _musaitlikService.GetTakvimAsync(personelId, yil, ay);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Belirli tarih aralığındaki müsaitlik kayıtlarını getirir
        /// </summary>
        /// <param name="personelId">Akademik personel ID'si</param>
        /// <param name="baslangic">Başlangıç tarihi (YYYY-MM-DD)</param>
        /// <param name="bitis">Bitiş tarihi (YYYY-MM-DD)</param>
        [HttpGet("personel/{personelId}/tarih-araligi")]
        public async Task<IActionResult> GetByDateRange(
            int personelId,
            [FromQuery] DateTime baslangic,
            [FromQuery] DateTime bitis)
        {
            if (baslangic > bitis)
                return BadRequest(new { Success = false, Message = "Başlangıç tarihi bitiş tarihinden sonra olamaz." });

            var result = await _musaitlikService.GetByDateRangeAsync(personelId, baslangic, bitis);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        #endregion

        #region Gözetmen Atama Endpoint'leri

        /// <summary>
        /// Belirli tarih ve saatte müsait olan personelleri getirir
        /// Sınav gözetmen ataması için kullanılır
        /// </summary>
        /// <param name="tarih">Sınav tarihi (YYYY-MM-DD)</param>
        /// <param name="baslangicSaati">Sınav başlangıç saati (HH:mm) - opsiyonel</param>
        /// <param name="bitisSaati">Sınav bitiş saati (HH:mm) - opsiyonel</param>
        [HttpGet("musait-personeller")]
        public async Task<IActionResult> GetMusaitPersoneller(
            [FromQuery] DateTime tarih,
            [FromQuery] string? baslangicSaati = null,
            [FromQuery] string? bitisSaati = null)
        {
            TimeSpan? baslangic = null;
            TimeSpan? bitis = null;

            // Saat string'lerini TimeSpan'e çevir
            if (!string.IsNullOrEmpty(baslangicSaati) && TimeSpan.TryParse(baslangicSaati, out var bs))
                baslangic = bs;

            if (!string.IsNullOrEmpty(bitisSaati) && TimeSpan.TryParse(bitisSaati, out var bt))
                bitis = bt;

            var result = await _musaitlikService.GetMusaitPersonellerAsync(tarih, baslangic, bitis);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Belirli tarih ve saatte meşgul olan personel ID'lerini getirir
        /// </summary>
        /// <param name="tarih">Tarih (YYYY-MM-DD)</param>
        /// <param name="baslangicSaati">Başlangıç saati (HH:mm) - opsiyonel</param>
        /// <param name="bitisSaati">Bitiş saati (HH:mm) - opsiyonel</param>
        [HttpGet("mesgul-personeller")]
        public async Task<IActionResult> GetMesgulPersoneller(
            [FromQuery] DateTime tarih,
            [FromQuery] string? baslangicSaati = null,
            [FromQuery] string? bitisSaati = null)
        {
            TimeSpan? baslangic = null;
            TimeSpan? bitis = null;

            if (!string.IsNullOrEmpty(baslangicSaati) && TimeSpan.TryParse(baslangicSaati, out var bs))
                baslangic = bs;

            if (!string.IsNullOrEmpty(bitisSaati) && TimeSpan.TryParse(bitisSaati, out var bt))
                bitis = bt;

            var result = await _musaitlikService.GetMesgulPersonelIdlerAsync(tarih, baslangic, bitis);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Personelin belirli tarih ve saatte meşgul olup olmadığını kontrol eder
        /// Gözetmen atama öncesi validasyon için kullanılır
        /// </summary>
        /// <param name="personelId">Akademik personel ID'si</param>
        /// <param name="tarih">Tarih (YYYY-MM-DD)</param>
        /// <param name="baslangicSaati">Başlangıç saati (HH:mm) - opsiyonel</param>
        /// <param name="bitisSaati">Bitiş saati (HH:mm) - opsiyonel</param>
        [HttpGet("kontrol/{personelId}")]
        public async Task<IActionResult> CheckMusaitlik(
            int personelId,
            [FromQuery] DateTime tarih,
            [FromQuery] string? baslangicSaati = null,
            [FromQuery] string? bitisSaati = null)
        {
            TimeSpan? baslangic = null;
            TimeSpan? bitis = null;

            if (!string.IsNullOrEmpty(baslangicSaati) && TimeSpan.TryParse(baslangicSaati, out var bs))
                baslangic = bs;

            if (!string.IsNullOrEmpty(bitisSaati) && TimeSpan.TryParse(bitisSaati, out var bt))
                bitis = bt;

            var result = await _musaitlikService.IsMesgulAsync(personelId, tarih, baslangic, bitis);
            if (result.Success)
            {
                return Ok(new
                {
                    Success = true,
                    IsMesgul = result.Data,
                    IsMusait = !result.Data,
                    Message = result.Message
                });
            }
            return BadRequest(result);
        }

        #endregion
    }
}
