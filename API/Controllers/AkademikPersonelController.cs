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

        [HttpPost]
        public IActionResult Add(AkademikPersonel akademikPersonel)
        {
            var result = _akademikPersonelService.Add(akademikPersonel);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }


        [HttpPut]
        public IActionResult Update(AkademikPersonel akademikPersonel)
        {
            var result = _akademikPersonelService.Update(akademikPersonel);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var akademikPersonel = _akademikPersonelService.GetById(id).Data;
            if (akademikPersonel == null)
                return NotFound(new { Success = false, Message = "Akademik personel bulunamadı" });

            var result = _akademikPersonelService.Delete(akademikPersonel);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

    }
} 