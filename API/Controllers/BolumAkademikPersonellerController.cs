using Business.Abstract;
using Entity.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/bolumakademikpersoneller")]
    [ApiController]
    public class BolumAkademikPersonellerController : ControllerBase
    {
        private IBolumAkademikPersonellerService _bolumAkademikPersonellerService;

        public BolumAkademikPersonellerController(IBolumAkademikPersonellerService bolumAkademikPersonellerService)
        {
            _bolumAkademikPersonellerService = bolumAkademikPersonellerService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _bolumAkademikPersonellerService.GetAll();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var result = _bolumAkademikPersonellerService.GetById(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost]
        public IActionResult Add(BolumAkademikPersoneller bolumAkademikPersoneller)
        {
            var result = _bolumAkademikPersonellerService.Add(bolumAkademikPersoneller);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpPut]
        public IActionResult Update(BolumAkademikPersoneller bolumAkademikPersoneller)
        {
            var result = _bolumAkademikPersonellerService.Update(bolumAkademikPersoneller);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete]
        public IActionResult Delete(BolumAkademikPersoneller bolumAkademikPersoneller)
        {
            var result = _bolumAkademikPersonellerService.Delete(bolumAkademikPersoneller);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }




        [HttpGet("bolum/{bolumId}/personeller")]
        public IActionResult GetAkademikPersonellerByBolumId(int bolumId)
        {
            var result = _bolumAkademikPersonellerService.GetAkademikPersonellerByBolumId(bolumId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("akademikpersonel/{akademikPersonelId}/bolumler")]
        public IActionResult GetBolumlerByAkademikPersonelId(int akademikPersonelId)
        {
            var result = _bolumAkademikPersonellerService.GetBolumlerByAkademikPersonelId(akademikPersonelId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
