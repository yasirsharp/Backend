using Microsoft.AspNetCore.Mvc;
using Business.Constants;
using Microsoft.AspNetCore.Authorization;
using Business.Abstract;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;
using Entity.DTOs;
using Core.Extensions;
using Core.Utilities.Results;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SinavDetayController : ControllerBase
    {
        private readonly ISinavDetayService _sinavDetayService;

        public SinavDetayController(ISinavDetayService sinavDetayService)
        {
            _sinavDetayService = sinavDetayService;
        }

        /// <summary>Öğrencinin token'ındaki bölüm (personel için null).</summary>
        private int? StudentBolumId() =>
            int.TryParse(User.FindFirst("BolumId")?.Value, out var id) ? id : null;

        /// <summary>Personel tüm bölümleri, öğrenci yalnızca kendi bölümünün sınavlarını görebilir.</summary>
        private bool CanSeeBolum(int bolumId) =>
            User.IsInAnyRole(Roles.Personel) || StudentBolumId() == bolumId;

        [HttpGet]
        [Authorize(Roles = Roles.Personel)]
        public IActionResult GetAll()
        {
            var result = _sinavDetayService.GetAll();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("paged")]
        [Authorize(Roles = Roles.Personel)]
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

            var result = _sinavDetayService.GetPagedList(paginationParams);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("details")]
        [Authorize(Roles = Roles.Personel)]
        public IActionResult GetAllDetails()
        {
            var result = _sinavDetayService.GetAllDetails();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Roles.Personel)]
        public IActionResult GetById(int id)
        {
            var result = _sinavDetayService.GetById(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("bolum/{bolumId}")]
        public IActionResult GetByBolumId(int bolumId)
        {
            if (!CanSeeBolum(bolumId))
                return Forbid();
            var result = _sinavDetayService.GetByBolumId(bolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("derslik/{derslikId}")]
        [Authorize(Roles = Roles.Personel)]
        public IActionResult GetByDerslikId(int derslikId)
        {
            var result = _sinavDetayService.GetByDerslikId(derslikId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("akademikpersonel/{akademikPersonelId}")]
        [Authorize(Roles = Roles.Personel)]
        public IActionResult GetByAkademikPersonelId(int akademikPersonelId)
        {
            var result = _sinavDetayService.GetByAkademikPersonelId(akademikPersonelId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
        
        [HttpGet("daterange")]
        [Authorize(Roles = Roles.Personel)]
        public IActionResult GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = _sinavDetayService.GetByDateRange(startDate, endDate);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
        
        [HttpGet("daterange-bolum")]
        public IActionResult GetByDateRangeAndBolum([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int? bolumId = null)
        {
            // Öğrenci bölüm vermezse token'daki kendi bölümü kullanılır
            bolumId ??= StudentBolumId();
            if (bolumId == null)
                return BadRequest(new ErrorResult("bolumId gerekli."));
            if (!CanSeeBolum(bolumId.Value))
                return Forbid();
            var result = _sinavDetayService.GetByDateRangeAndBolum(startDate, endDate, bolumId.Value);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
        
        [HttpGet("daterange-derslik")]
        [Authorize(Roles = Roles.Personel)]
        public IActionResult GetByDateRangeAndDerslik([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int derslikId)
        {
            var result = _sinavDetayService.GetByDateRangeAndDerslik(startDate, endDate, derslikId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
        
        [HttpGet("daterange-akademikpersonel")]
        [Authorize(Roles = Roles.Personel)]
        public IActionResult GetByDateRangeAndAkademikPersonel([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int akademikPersonelId)
        {
            var result = _sinavDetayService.GetByDateRangeAndAkademikPersonel(startDate, endDate, akademikPersonelId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("my-exams")]
        [Authorize(Roles = Roles.Personel)]
        public IActionResult GetMyExams([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            // Token'dan userId'yi al
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı." });
            }

            var result = _sinavDetayService.GetMyExamsForUser(userId, startDate, endDate);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }
        
        [HttpGet("derslikler")]
        [Authorize(Roles = Roles.Personel)]
        public IActionResult GetByDerslikler([FromQuery] int[] ids)
        {
            var result = _sinavDetayService.GetByDerslikler(ids);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("derslikler-bolum")]
        [Authorize(Roles = Roles.Personel)]
        public IActionResult GetByDersliklerAndBolum([FromQuery] int[] ids, [FromQuery] int bolumId)
        {
            var result = _sinavDetayService.GetByDersliklerAndBolum(ids, bolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("derslikler-akademikpersonel")]
        [Authorize(Roles = Roles.Personel)]
        public IActionResult GetByDersliklerAndAkademikPersonel([FromQuery] int[] ids, [FromQuery] int akademikPersonelId)
        {
            var result = _sinavDetayService.GetByDersliklerAndAkademikPersonel(ids, akademikPersonelId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost]
        [Authorize(Roles = Roles.Planlayici)]
        public async Task<IActionResult> Add(SinavKayitDTO sinavKayitDTO)
        {
            var result = await _sinavDetayService.Add(sinavKayitDTO);
            if (result.Success)
                return Ok(result); // 200 OK (diğer endpoint'lerle tutarlı)
            return BadRequest(result);
        }

        [HttpPut]
        [Authorize(Roles = Roles.Planlayici)]
        public async Task<IActionResult> Update(SinavGuncelleDTO sinavGuncelleDTO)
        {
            var result = await _sinavDetayService.Update(sinavGuncelleDTO);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult Delete(int id)
        {
            var result = _sinavDetayService.Delete(new SinavDetay { Id = id });
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
} 