using Microsoft.AspNetCore.Mvc;
using Business.Abstract;
using Entity.Concrete;
using Entity.DTOs;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DersController : ControllerBase
    {
        private readonly IDersService _dersService;

        public DersController(IDersService dersService)
        {
            _dersService = dersService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _dersService.GetList();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var result = _dersService.GetById(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost]
        public IActionResult Add(Ders ders)
        {
            var result = _dersService.Add(ders);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpPut]
        public IActionResult Update(Ders ders)
        {
            var result = _dersService.Update(ders);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete]
        public IActionResult Delete(Ders ders)
        {
            var result = _dersService.Delete(ders);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("with-bolumler")]
        public IActionResult AddWithBolumler(DersEkleDTO dersEkleDto)
        {
            var result = _dersService.AddDersWithBolumler(dersEkleDto);
            if (result.Success)
                return Created("", result);
            return BadRequest(result);
        }

        [HttpGet("with-bolumler")]
        public IActionResult GetAllWithBolumler()
        {
            var result = _dersService.GetAllWithBolumler();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPut("with-bolumler")]
        public IActionResult UpdateWithBolumler(DersGuncelleDTO dersGuncelleDto)
        {
            var result = _dersService.UpdateDersWithBolumler(dersGuncelleDto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
} 