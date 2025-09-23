using Microsoft.AspNetCore.Mvc;
using Business.Abstract;
using Entity.Concrete;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SinavDerslikController : ControllerBase
    {
        private readonly ISinavDerslikService _sinavDerslikService;

        public SinavDerslikController(ISinavDerslikService sinavDerslikService)
        {
            _sinavDerslikService = sinavDerslikService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _sinavDerslikService.GetAll();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var result = _sinavDerslikService.Get(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("derslik/{derslikId}")]
        public IActionResult GetByDerslikId(int derslikId)
        {
            var result = _sinavDerslikService.GetByDerslikId(derslikId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("gozetmen/{gozetmenId}")]
        public IActionResult GetByGozetmenId(int gozetmenId)
        {
            var result = _sinavDerslikService.GetByGozetmenId(gozetmenId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("sinavdetay/{sinavDetayId}")]
        public IActionResult GetBySinavDetayId(int sinavDetayId)
        {
            var result = _sinavDerslikService.GetBySinavDetayId(sinavDetayId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost]
        public IActionResult Add(SinavDerslik sinavDerslik)
        {
            var result = _sinavDerslikService.Add(sinavDerslik);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpPut]
        public IActionResult Update(SinavDerslik sinavDerslik)
        {
            var result = _sinavDerslikService.Update(sinavDerslik);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete]
        public IActionResult Delete(SinavDerslik sinavDerslik)
        {
            var result = _sinavDerslikService.Delete(sinavDerslik);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
} 