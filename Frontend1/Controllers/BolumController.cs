using Business.Abstract;
using Business.Concrete;
using DataAccess.Concrete;
using Entity.Concrete;
using Microsoft.AspNetCore.Mvc;

namespace Frontend1.Controllers
{
    public class BolumController : Controller
    {
        IBolumService _bolumService;

        public BolumController(IBolumService bolumService)
        {
            _bolumService = bolumService;
        }

        [HttpGet]
        [Route("Bolum/index")]
        [Route("Bolum/Yonetim")]
        [Route("Bolum/BolumYonetim")]
        public IActionResult Index()
        {
            return View(_bolumService.GetList().Data);
        }
        [HttpPost]
        [Route("/Bolum/Add")]
        public IActionResult Add([FromBody] Bolum bolum)
        {
            var result = _bolumService.Add(bolum);
            return Ok(result);
        }



        [HttpPost]
        public IActionResult Delete([FromBody] Bolum bolum)
        {
            var result = _bolumService.Delete(bolum);
            if (result.Success)
            {
                return Ok(result); // Başarılı yanıt
            }
            return BadRequest(result);

        }
        [HttpPost]
        public IActionResult Update([FromBody] Bolum bolum)
        {
            var _bolum = _bolumService.GetById(bolum.Id);
            if (_bolum.Data == null)
            {
                return BadRequest(new { message = _bolum.Message });
            }

            var result = _bolumService.Update(bolum);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result); // Başarılı yanıt
        }
        [HttpGet]
        [Route("Bolum/GetById/{id}")]
        public IActionResult GetById(int id)
        {
            var bolum = _bolumService.GetById(id);
            if (bolum.Success)
            {
                return Ok(bolum); // Bolum nesnesini JSON olarak döner
            }
            return BadRequest(new { message = bolum.Message });
        }
    }
}
