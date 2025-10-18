using Microsoft.AspNetCore.Mvc;
using Business.Abstract;
using Entity.Concrete;

namespace API.Controllers
{
    [Route("api/dersbolumakademikpersoneller")]
    [ApiController]
    public class DBAPController : ControllerBase
    {
        private readonly IDBAPService _dbapService;

        public DBAPController(IDBAPService dbapService)
        {
            _dbapService = dbapService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _dbapService.GetAll();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("details")]
        public IActionResult GetAllDetails()
        {
            var result = _dbapService.GetAllDetails();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var result = _dbapService.GetById(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("detail/{id}")]
        public IActionResult GetDetail(int id)
        {
            var result = _dbapService.GetDetail(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("bolum/{bolumId}")]
        public IActionResult GetByBolumId(int bolumId)
        {
            var result = _dbapService.GetByBolumId(bolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("bolum/details/{bolumId}")]
        public IActionResult GetDetailsByBolumId(int bolumId)
        {
            var result = _dbapService.GetDetailsByBolumId(bolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost]
        public IActionResult Add(DersBolumAkademikPersonel dersBolumAkademikPersonel)
        {
            var result = _dbapService.Add(dersBolumAkademikPersonel);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpPut]
        public IActionResult Update(DersBolumAkademikPersonel dersBolumAkademikPersonel)
        {
            var result = _dbapService.Update(dersBolumAkademikPersonel);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var dbap = _dbapService.GetById(id).Data;
            if (dbap == null)
                return NotFound(new { Success = false, Message = "DBAP kaydı bulunamadı" });

            var result = _dbapService.Delete(dbap);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
} 