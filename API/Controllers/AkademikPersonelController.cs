using Microsoft.AspNetCore.Mvc;
using Business.Abstract;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;
using System.Linq.Expressions;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AkademikPersonelController : ControllerBase
    {
        private readonly IAkademikPersonelService _akademikPersonelService;

        public AkademikPersonelController(IAkademikPersonelService akademikPersonelService)
        {
            _akademikPersonelService = akademikPersonelService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _akademikPersonelService.GetList();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("paged")]
        public IActionResult GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? searchTerm = null)
        {
            var paginationParams = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder,
                SearchTerm = searchTerm
            };

            var result = _akademikPersonelService.GetPagedList(paginationParams);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var result = _akademikPersonelService.GetById(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("by-user-id/{userId}")]
        public IActionResult GetByUserId(int userId)
        {
            var result = _akademikPersonelService.GetByUserId(userId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        /// <summary>
        /// Belirli bir bölüme ait akademik personelleri getirir (GorevliLayout için)
        /// BolumAkademikPersoneller tablosu üzerinden çalışır
        /// </summary>
        [HttpGet("bolum/{bolumId}")]
        public IActionResult GetByBolumId(int bolumId)
        {
            // Tüm akademik personelleri al
            var allPersonel = _akademikPersonelService.GetList();
            if (!allPersonel.Success)
                return BadRequest(allPersonel);

            // TODO: Service layer'da BolumAkademikPersoneller ile join yapan metod olmalı
            // Şimdilik basit liste dönüyoruz
            return Ok(new Core.Utilities.Results.SuccessDataResult<List<AkademikPersonel>>(
                allPersonel.Data,
                $"{allPersonel.Data.Count} akademik personel bulundu. (BolumId filtresi uygulanmadı - Service layer'da implement edilmeli)"
            ));
        }

        [HttpPost]
        public async Task<IActionResult> Add(AkademikPersonel akademikPersonel)
        {
            var result = await _akademikPersonelService.Add(akademikPersonel);

            if (result.Success)
                return Created("", result);

            return BadRequest(result);
        }
        [HttpPut]
        public async Task<IActionResult> Update(AkademikPersonel akademikPersonel)
        {
            var result = await _akademikPersonelService.Update(akademikPersonel);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var akademikPersonelResult = _akademikPersonelService.GetById(id);

            if (!akademikPersonelResult.Success || akademikPersonelResult.Data == null)
                return NotFound(new { Success = false, Message = "Akademik personel bulunamadı" });

            var result = await _akademikPersonelService.Delete(akademikPersonelResult.Data);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }


    }
} 