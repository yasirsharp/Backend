using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Abstract;
using Entity.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    /// <summary>
    /// Akademik personel müsaitlik yönetimi API endpoint'leri
    /// Sınav gözetmen ataması için müsaitlik kontrolü sağlar
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AkademikPersonelMusaitlikController : ControllerBase
    {
        private readonly IAkademikPersonelMusaitlikService _musaitlikService;

        public AkademikPersonelMusaitlikController(IAkademikPersonelMusaitlikService musaitlikService)
        {
            _musaitlikService = musaitlikService;
        }

        #region CRUD Endpoint'leri

        /// <summary>
        /// Yeni müsaitlik kaydı ekler (çakışma kontrolü ile)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] MusaitlikCreateDto dto)
        {
            var result = await _musaitlikService.AddAsync(dto);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        /// <summary>
        /// Müsaitlik kaydını günceller
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] MusaitlikUpdateDto dto)
        {
            var result = await _musaitlikService.UpdateAsync(dto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Müsaitlik kaydını siler (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _musaitlikService.DeleteAsync(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Toplu silme - birden fazla kaydı tek seferde siler
        /// </summary>
        [HttpPost("batch-delete")]
        public async Task<IActionResult> DeleteBatch([FromBody] List<int> ids)
        {
            var result = await _musaitlikService.DeleteBatchAsync(ids);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// ID'ye göre müsaitlik kaydını getirir
        /// </summary>
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
        /// </summary>
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
        /// </summary>
        [HttpGet("musait-personeller")]
        public async Task<IActionResult> GetMusaitPersoneller(
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

            var result = await _musaitlikService.GetMusaitPersonellerAsync(tarih, baslangic, bitis);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Belirli tarih ve saatte meşgul olan personel ID'lerini getirir
        /// </summary>
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
        /// </summary>
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
