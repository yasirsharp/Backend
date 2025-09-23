using Microsoft.AspNetCore.Mvc;
using Business.Abstract;
using Entity.Concrete;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DerslikController : ControllerBase
    {
        private readonly IDerslikService _derslikService;

        public DerslikController(IDerslikService derslikService)
        {
            _derslikService = derslikService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _derslikService.GetList();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var result = _derslikService.GetById(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost]
        public IActionResult Add(Derslik derslik)
        {
            var result = _derslikService.Add(derslik);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpPut]
        public IActionResult Update(Derslik derslik)
        {
            var result = _derslikService.Update(derslik);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete]
        public IActionResult Delete(Derslik derslik)
        {
            var result = _derslikService.Delete(derslik);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
} 