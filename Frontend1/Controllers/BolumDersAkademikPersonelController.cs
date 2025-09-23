using Business.Abstract;
using Core.Utilities.Results;
using Entity.Concrete;
using Entity.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Frontend1.Controllers
{
    public class BolumDersAkademikPersonelController : Controller
    {
        IBolumService _bolumService;
        IDersService _dersService;
        IAkademikPersonelService _akademikPersonelService;
        IDBAPService _dbapService;

        public BolumDersAkademikPersonelController(IDBAPService dBAPService, IBolumService bolumService, IDersService dersService, IAkademikPersonelService akademikPersonelService)
        {
            _bolumService = bolumService;
            _dersService = dersService;
            _akademikPersonelService = akademikPersonelService;
            _dbapService = dBAPService;
        }

        [HttpGet]
        [Route("BolumDersAkademikPersonel/index")]
        public IActionResult Index()
        {
            ViewData["Bolumler"] = _bolumService.GetList();
            ViewData["Dersler"] = _dersService.GetList();
            ViewData["APler"] = _akademikPersonelService.GetList();
            ViewData["BDAP"] = _dbapService.GetAllDetails();

            return View();
        }

        [HttpGet]
        [Route("BolumDersAkademikPersonel/GetById/{id}")]
        public IActionResult GetById(int id)
        {
            var result = _dbapService.GetById(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("BolumDersAkademikPersonel/Add")]
        public IActionResult Add([FromBody] DersBolumAkademikPersonel model)
        {
            var result = _dbapService.Add(model);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("BolumDersAkademikPersonel/Update")]
        public IActionResult Update([FromBody] DersBolumAkademikPersonel model)
        {
            var result = _dbapService.Update(model);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("BolumDersAkademikPersonel/Delete")]
        public IActionResult Delete([FromBody] DersBolumAkademikPersonel model)
        {
            try
            {
                var result = _dbapService.Delete(model);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception err)
            {
                return BadRequest(new ErrorResult(err.Message));
            }
        }

        [HttpGet]
        [Route("/BolumDersAkademikPersonel/GetDetail/{id}")]
        public IActionResult GetDetail(int id)
        {
            var result = _dbapService.GetDetail(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("BolumDersAkademikPersonel/GetAllDetails")]
        public IActionResult GetAllDetails()
        {
            var result = _dbapService.GetAllDetails();
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
