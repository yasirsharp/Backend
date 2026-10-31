using Microsoft.AspNetCore.Mvc;
using Business.Constants;
using Microsoft.AspNetCore.Authorization;
using Business.Abstract;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;
using Entity.DTOs;
using System.Linq.Expressions;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.Personel)] // Öğrenciler personel listesine erişemez
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
        /// Bölümde ders veren (ders atamaları) veya bölüm görevlisi olan akademik personel.
        /// </summary>
        [HttpGet("bolum/{bolumId}")]
        public IActionResult GetByBolumId(int bolumId)
        {
            var result = _akademikPersonelService.GetByBolumId(bolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Personel ve kullanıcı hesabı açar. Gövde: { ad, unvan, email? }. Geçici parola yanıtta bir kez döner.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = Roles.Yonetici)]
        public async Task<IActionResult> Add(AkademikPersonelCreateDto dto)
        {
            var result = await _akademikPersonelService.CreateAsync(dto);

            if (result.Success)
                return Created("", result);

            return BadRequest(result);
        }

        /// <summary>Toplu personel ekleme (en fazla 1000 satır), satır bazında sonuç döner.</summary>
        [HttpPost("bulk")]
        [Authorize(Roles = Roles.Yonetici)]
        [RequestSizeLimit(2_000_000)]
        public async Task<IActionResult> BulkAdd(List<AkademikPersonelCreateDto> rows)
        {
            var result = await _akademikPersonelService.BulkCreateAsync(rows);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPut]
        [Authorize(Roles = Roles.Yonetici)]
        public async Task<IActionResult> Update(AkademikPersonel akademikPersonel)
        {
            var result = await _akademikPersonelService.Update(akademikPersonel);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Yonetici)]
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