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
    public class DersController : ControllerBase
    {
        private readonly IDersService _dersService;
        private readonly BolumScope _scope;

        public DersController(IDersService dersService, BolumScope scope)
        {
            _dersService = dersService;
            _scope = scope;
        }

        private IActionResult OwnBolumOnly() => StatusCode(StatusCodes.Status403Forbidden, new Core.Utilities.Results.ErrorResult(BolumScope.OwnBolumOnly));
        private IActionResult SharedAdminOnly() => StatusCode(StatusCodes.Status403Forbidden, new Core.Utilities.Results.ErrorResult(BolumScope.SharedAdminOnly));

        [HttpGet]
        public IActionResult GetAll()
        {
            if (_scope.IsScoped)
                return OwnBolumOnly();
            var result = _dersService.GetList();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Sayfalanmış, sıralanmış ve filtrelenmiş ders listesi
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

            var result = _dersService.GetPagedList(paginationParams);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            if (_scope.IsScoped && !(BolumScope.BolumsOfDers(id) is { } bolumler && _scope.CanSee(bolumler)))
                return OwnBolumOnly();
            var result = _dersService.GetById(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Belirli bir bölüme ait dersleri getirir (GorevliLayout için)
        /// </summary>
        [HttpGet("bolum/{bolumId}")]
        public IActionResult GetByBolumId(int bolumId)
        {
            if (!_scope.CanAccess(bolumId))
                return OwnBolumOnly();
            // Bölüme ait tüm DBAP kayıtlarından ders ID'lerini çek
            // Bu işlem için service layer'da yeni metod gerekebilir
            // Şimdilik DersWithBolumler'i kullanabiliriz
            var allDersler = _dersService.GetAllWithBolumler();
            if (!allDersler.Success)
                return BadRequest(allDersler);

            // BolumId'ye göre filtrele
            var filteredDersler = allDersler.Data
                .Where(d => d.Bolumler.Any(b => b.BolumId == bolumId))
                .ToList();

            return Ok(new Core.Utilities.Results.SuccessDataResult<List<DersWithBolumlerDTO>>(
                filteredDersler,
                $"{filteredDersler.Count} ders bulundu."
            ));
        }

        [HttpPost]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult Add(Ders ders)
        {
            // Bölüm bilgisi taşımayan eski uç: yalnızca yönetici
            if (_scope.IsScoped)
                return SharedAdminOnly();
            var result = _dersService.Add(ders);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpPut]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult Update(Ders ders)
        {
            // Bölüm bilgisi taşımayan eski uç: yalnızca yönetici
            if (_scope.IsScoped)
                return SharedAdminOnly();
            var result = _dersService.Update(ders);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult Delete(int id)
        {
            if (_scope.IsScoped && !(BolumScope.BolumsOfDers(id) is { } bolumler && _scope.CanManage(bolumler)))
                return SharedAdminOnly();
            var ders = _dersService.GetById(id);
            if (!ders.Success)
                return NotFound(ders);
            
            var result = _dersService.Delete(ders.Data);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("with-bolumler")]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult AddWithBolumler(DersEkleDTO dersEkleDto)
        {
            if (!_scope.CanManage(dersEkleDto.OrtakDers ? new List<int>() : dersEkleDto.BolumIds ?? new List<int>()))
                return SharedAdminOnly();
            var result = _dersService.AddDersWithBolumler(dersEkleDto);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpGet("with-bolumler")]
        public IActionResult GetAllWithBolumler()
        {
            var result = _dersService.GetAllWithBolumler();
            if (result.Success && _scope.IsScoped)
            {
                // Görevli: kendi bölümlerinin dersleri + ortak dersler
                var list = result.Data.Where(d => _scope.CanSee(d.Bolumler.Select(b => b.BolumId).ToList())).ToList();
                return Ok(new Core.Utilities.Results.SuccessDataResult<List<DersWithBolumlerDTO>>(list, result.Message));
            }
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPut("with-bolumler")]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult UpdateWithBolumler(DersGuncelleDTO dersGuncelleDto)
        {
            // Hem şimdiki hem yeni bölümlerin hepsi kendi bölümü olmalı
            if (_scope.IsScoped && !(BolumScope.BolumsOfDers(dersGuncelleDto.Id) is { } eski && _scope.CanManage(eski)
                                     && _scope.CanManage(dersGuncelleDto.OrtakDers ? new List<int>() : dersGuncelleDto.BolumIds ?? new List<int>())))
                return SharedAdminOnly();
            var result = _dersService.UpdateDersWithBolumler(dersGuncelleDto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("check-kod-unique")]
        public IActionResult CheckKodUnique([FromQuery] string kod, [FromQuery] int? excludeDersId = null)
        {
            var result = _dersService.IsKodUnique(kod, excludeDersId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Ders-Bölüm eşleştirmesini siler. Eğer son eşleştirme ise dersi de siler.
        /// </summary>
        [HttpDelete("remove-bolum-mapping/{dersId}/{bolumId}")]
        [Authorize(Roles = Roles.Planlayici)]
        public IActionResult RemoveDersBolumMapping(int dersId, int bolumId)
        {
            if (_scope.IsScoped && !(BolumScope.BolumsOfDers(dersId) is { } bolumler && _scope.CanManage(bolumler)))
                return SharedAdminOnly();
            var result = _dersService.RemoveDersBolumMapping(dersId, bolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
} 