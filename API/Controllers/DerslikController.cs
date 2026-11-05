using Microsoft.AspNetCore.Mvc;
using Business.Constants;
using Microsoft.AspNetCore.Authorization;
using Business.Abstract;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;
using Entity.DTOs;

using API.Security;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DerslikController : ControllerBase
    {
        private readonly IDerslikService _derslikService;
        private readonly BolumScope _scope;

        public DerslikController(IDerslikService derslikService, BolumScope scope)
        {
            _derslikService = derslikService;
            _scope = scope;
        }

        private IActionResult OwnBolumOnly() => StatusCode(StatusCodes.Status403Forbidden, new Core.Utilities.Results.ErrorResult(BolumScope.OwnBolumOnly));
        private IActionResult SharedAdminOnly() => StatusCode(StatusCodes.Status403Forbidden, new Core.Utilities.Results.ErrorResult(BolumScope.SharedAdminOnly));

        [HttpGet]
        public IActionResult GetAll()
        {
            if (_scope.IsScoped)
                return OwnBolumOnly();
            var result = _derslikService.GetList();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Sayfalanmış, sıralanmış ve filtrelenmiş derslik listesi
        /// </summary>
        [HttpGet("paged")]
        public IActionResult GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = "Id",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] string? searchTerm = null)
        {
            if (_scope.IsScoped)
                return OwnBolumOnly();
            var paginationParams = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder,
                SearchTerm = searchTerm
            };

            var result = _derslikService.GetPagedList(paginationParams);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("with-bolumler")]
        public IActionResult GetAllWithBolumler()
        {
            var result = _derslikService.GetAllWithBolumler();
            if (!result.Success)
                return BadRequest(result);
            // Görevli: kendi bölümlerinin derslikleri + ortak derslikler
            if (_scope.IsScoped)
                return Ok(new Core.Utilities.Results.SuccessDataResult<List<DerslikWithBolumlerDTO>>(
                    result.Data.Where(d => _scope.CanSee(d.Bolumler.Select(b => b.BolumId).ToList())).ToList(), result.Message));
            return Ok(result);
        }

        /// <summary>
        /// Sayfalanmış derslik listesi (bölümlerle birlikte)
        /// </summary>
        [HttpGet("with-bolumler/paged")]
        public IActionResult GetPagedWithBolumler(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = "Id",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] string? searchTerm = null)
        {
            if (_scope.IsScoped)
                return OwnBolumOnly();
            var paginationParams = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder,
                SearchTerm = searchTerm
            };

            // Tüm derslikleri bölümlerle birlikte getir
            var allResult = _derslikService.GetAllWithBolumler();
            if (!allResult.Success)
                return BadRequest(allResult);

            var allDerslikler = allResult.Data;

            // Frontend'den gelen arama terimine göre filtrele
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                allDerslikler = allDerslikler
                    .Where(d => d.DerslikAd.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Sıralama
            allDerslikler = sortBy?.ToLower() switch
            {
                "ad" or "derslikad" => sortOrder.ToLower() == "desc"
                    ? allDerslikler.OrderByDescending(d => d.DerslikAd).ToList()
                    : allDerslikler.OrderBy(d => d.DerslikAd).ToList(),
                "kapasite" => sortOrder.ToLower() == "desc"
                    ? allDerslikler.OrderByDescending(d => d.Kapasite).ToList()
                    : allDerslikler.OrderBy(d => d.Kapasite).ToList(),
                _ => sortOrder.ToLower() == "desc"
                    ? allDerslikler.OrderByDescending(d => d.DerslikId).ToList()
                    : allDerslikler.OrderBy(d => d.DerslikId).ToList()
            };

            // Pagination
            var totalItems = allDerslikler.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var pagedItems = allDerslikler
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagedResult = new
            {
                Data = pagedItems,
                Success = true,
                Message = $"{pagedItems.Count} derslik bulundu (Toplam: {totalItems})",
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };

            return Ok(pagedResult);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            if (_scope.IsScoped && !(BolumScope.BolumsOfDerslik(id) is { } bolumler && _scope.CanSee(bolumler)))
                return OwnBolumOnly();
            var result = _derslikService.GetById(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult Add(Derslik derslik)
        {
            // Bölüm bilgisi taşımayan eski uç: yalnızca yönetici
            if (_scope.IsScoped)
                return SharedAdminOnly();
            var result = _derslikService.Add(derslik);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpPost("with-bolumler")]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult AddWithBolumler([FromBody] DerslikEkleDTO derslikEkleDto)
        {
            if (!_scope.CanManage(derslikEkleDto.OrtakDerslik ? new List<int>() : derslikEkleDto.BolumIds ?? new List<int>()))
                return SharedAdminOnly();
            var result = _derslikService.AddDerslikWithBolumler(derslikEkleDto);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpPut]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult Update(Derslik derslik)
        {
            // Bölüm bilgisi taşımayan eski uç: yalnızca yönetici
            if (_scope.IsScoped)
                return SharedAdminOnly();
            var result = _derslikService.Update(derslik);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPut("with-bolumler")]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult UpdateWithBolumler([FromBody] DerslikGuncelleDTO derslikGuncelleDto)
        {
            // Hem şimdiki hem yeni bölümlerin hepsi kendi bölümü olmalı (ortak derslik yalnızca yönetici)
            if (_scope.IsScoped && !(BolumScope.BolumsOfDerslik(derslikGuncelleDto.Id) is { } eski && _scope.CanManage(eski)
                                     && _scope.CanManage(derslikGuncelleDto.OrtakDerslik ? new List<int>() : derslikGuncelleDto.BolumIds ?? new List<int>())))
                return SharedAdminOnly();
            var result = _derslikService.UpdateDerslikWithBolumler(derslikGuncelleDto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult Delete(int id)
        {
            if (_scope.IsScoped && !(BolumScope.BolumsOfDerslik(id) is { } bolumler && _scope.CanManage(bolumler)))
                return SharedAdminOnly();
            var derslik = _derslikService.GetById(id).Data;
            if (derslik == null)
                return NotFound(new { Success = false, Message = "Derslik bulunamadı" });
                
            var result = _derslikService.Delete(derslik);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Sadece belirtilen bölümden derslik ilişkisini sil
        /// </summary>
        [HttpDelete("{derslikId}/bolum/{bolumId}")]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult RemoveFromBolum(int derslikId, int bolumId)
        {
            if (_scope.IsScoped && !(BolumScope.BolumsOfDerslik(derslikId) is { } bolumler && _scope.CanManage(bolumler)))
                return SharedAdminOnly();
            var result = _derslikService.RemoveDerslikFromBolum(derslikId, bolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Belirli bir bölüme ait derslikleri getir (bolumId=0 ise ortak derslikler)
        /// </summary>
        [HttpGet("by-bolum/{bolumId}")]
        public IActionResult GetByBolumId(int bolumId)
        {
            if (bolumId != 0 && !_scope.CanAccess(bolumId))
                return OwnBolumOnly();
            var result = _derslikService.GetDersliklerByBolumId(bolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Ortak derslikleri getir (birden fazla bölüme ait veya hiçbir bölüme atanmamış)
        /// </summary>
        [HttpGet("ortak")]
        public IActionResult GetOrtakDerslikler()
        {
            var result = _derslikService.GetOrtakDerslikler();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Görevlinin bölümlerine ait derslikleri getir (ortak derslikler hariç)
        /// </summary>
        [HttpGet("by-bolumler")]
        public IActionResult GetByBolumIds([FromQuery] int[] bolumIds)
        {
            if (_scope.IsScoped)
                bolumIds = bolumIds.Where(_scope.CanAccess).ToArray();
            var result = _derslikService.GetDersliklerByBolumIds(bolumIds);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
} 