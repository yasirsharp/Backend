using Business.Abstract;
using Business.Concrete;
using DataAccess.Concrete;
using Entity.Concrete;
using Microsoft.AspNetCore.Mvc;

namespace Frontend1.Controllers
{
    public class DersController : Controller
    {
        IDersService _dersService;

        public DersController(IDersService dersService)
        {
            _dersService = dersService;
        }

        [HttpGet]
        [Route("Ders/index")]
        [Route("Ders/Yonetim")]
        [Route("Ders/DersYonetim")]
        public IActionResult Index()
        {
            return View(_dersService.GetList().Data);
        }
        [HttpPost]
        [Route("/Ders/Add")]
        public IActionResult Add([FromBody] Ders ders)
        {
            var result = _dersService.Add(ders);
            if(result.Success) return Ok(result);

            return BadRequest(new { message = result.Message });
        }



        [HttpPost]
        public IActionResult Delete([FromBody] Ders ders)
        {
            var result = _dersService.Delete(ders);
            if (result.Success) return Ok(result);

            return BadRequest(result.Message);
        }

        [HttpGet]
        public IActionResult GetById(int id)
        {
            var ders = _dersService.GetById(id);
            if (ders.Data != null)
            {
                return Ok(ders); // Ders nesnesini JSON olarak döner
            }
            return NotFound();
        }
        [HttpPost]
        public IActionResult Update([FromBody] Ders ders)
        {
            if (ders == null)
            {
                return BadRequest("Ders verisi eksik veya geçersiz.");
            }

            var result = _dersService.Update(ders);
            if (result.Success) return Ok(result);

            return BadRequest(new { message = result.Message });
        }
    }
}
