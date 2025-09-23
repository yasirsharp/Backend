using Business.Abstract;
using Business.Concrete;
using DataAccess.Concrete;
using Entity.Concrete;
using Microsoft.AspNetCore.Mvc;

namespace Frontend1.Controllers
{
    public class DerslikController : Controller
    {
        IDerslikService _derslikService;

        public DerslikController(IDerslikService derslikService)
        {
            _derslikService = derslikService;
        }

        [HttpGet]
        [Route("Derslik/index")]
        [Route("Derslik/Yonetim")]
        [Route("Derslik/DerslikYonetim")]
        public IActionResult Index()
        {
            return View(_derslikService.GetList().Data);
        }

        [HttpGet]
        [Route("Derslik/GetById/{id}")]
        public IActionResult GetById(int id)
        {
            var result = _derslikService.GetById(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("/Derslik/Add")]
        public IActionResult Add([FromBody] Derslik derslik)
        {
            var result = _derslikService.Add(derslik);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("/Derslik/Update")]
        public IActionResult Update([FromBody] Derslik derslik)
        {
            var result = _derslikService.Update(derslik);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("/Derslik/Delete")]
        public IActionResult Delete([FromBody] Derslik derslik)
        {
            var result = _derslikService.Delete(derslik);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
