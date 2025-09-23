using Microsoft.AspNetCore.Mvc;
using Business.Abstract;
using Entity.Concrete;
using Entity.DTOs;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SinavDetayController : ControllerBase
    {
        private readonly ISinavDetayService _sinavDetayService;

        public SinavDetayController(ISinavDetayService sinavDetayService)
        {
            _sinavDetayService = sinavDetayService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _sinavDetayService.GetAll();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("details")]
        public IActionResult GetAllDetails()
        {
            var result = _sinavDetayService.GetAllDetails();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
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
            var result = _sinavDetayService.GetByBolumId(bolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("derslik/{derslikId}")]
        public IActionResult GetByDerslikId(int derslikId)
        {
            var result = _sinavDetayService.GetByDerslikId(derslikId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("akademikpersonel/{akademikPersonelId}")]
        public IActionResult GetByAkademikPersonelId(int akademikPersonelId)
        {
            var result = _sinavDetayService.GetByAkademikPersonelId(akademikPersonelId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
        
        [HttpGet("daterange")]
        public IActionResult GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = _sinavDetayService.GetByDateRange(startDate, endDate);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
        
        [HttpGet("daterange-bolum")]
        public IActionResult GetByDateRangeAndBolum([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int bolumId)
        {
            var result = _sinavDetayService.GetByDateRangeAndBolum(startDate, endDate, bolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
        
        [HttpGet("daterange-derslik")]
        public IActionResult GetByDateRangeAndDerslik([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int derslikId)
        {
            var result = _sinavDetayService.GetByDateRangeAndDerslik(startDate, endDate, derslikId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
        
        [HttpGet("daterange-akademikpersonel")]
        public IActionResult GetByDateRangeAndAkademikPersonel([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int akademikPersonelId)
        {
            var result = _sinavDetayService.GetByDateRangeAndAkademikPersonel(startDate, endDate, akademikPersonelId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
        
        [HttpGet("derslikler")]
        public IActionResult GetByDerslikler([FromQuery] int[] ids)
        {
            var result = _sinavDetayService.GetByDerslikler(ids);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("derslikler-bolum")]
        public IActionResult GetByDersliklerAndBolum([FromQuery] int[] ids, [FromQuery] int bolumId)
        {
            var result = _sinavDetayService.GetByDersliklerAndBolum(ids, bolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("derslikler-akademikpersonel")]
        public IActionResult GetByDersliklerAndAkademikPersonel([FromQuery] int[] ids, [FromQuery] int akademikPersonelId)
        {
            var result = _sinavDetayService.GetByDersliklerAndAkademikPersonel(ids, akademikPersonelId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost]
        public IActionResult Add(SinavKayitDTO sinavKayitDTO)
        {
            var result = _sinavDetayService.Add(sinavKayitDTO);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpPut]
        public IActionResult Update(SinavGuncelleDTO sinavGuncelleDTO)
        {
            var result = _sinavDetayService.Update(sinavGuncelleDTO);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete]
        public IActionResult Delete(SinavDetay sinavDetay)
        {
            var result = _sinavDetayService.Delete(sinavDetay);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
} 