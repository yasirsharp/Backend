using Microsoft.AspNetCore.Mvc;
using Business.Abstract;
using Entity.Concrete;
using Core.Utilities.Results;
using Entity.DTOs;
using System.Text.Json;

namespace Frontend1.Controllers
{
    public class SinavDetayController : Controller
    {
        private readonly ISinavDetayService _sinavDetayService;
        private readonly IDBAPService _dBAPService;
        private readonly IDerslikService _derslikService;
        private readonly IAkademikPersonelService _akademikPersonelService;

        public SinavDetayController(
            ISinavDetayService sinavDetayService, 
            IDBAPService dBAPService, 
            IDerslikService derslikService,
            IAkademikPersonelService akademikPersonelService)
        {
            _sinavDetayService = sinavDetayService;
            _dBAPService = dBAPService;
            _derslikService = derslikService;
            _akademikPersonelService = akademikPersonelService;
        }

        [HttpGet]
        [Route("/sinavdetay/{bolumId?}")]
        public IActionResult Index(int? bolumId)
        {
            try
            {
                ViewData["DBAP"] = _dBAPService.GetByBolumId(bolumId ?? 0);
                ViewData["DBAPDetail"] = _dBAPService.GetDetailsByBolumId(bolumId ?? 0);
                ViewData["Derslikler"] = _derslikService.GetList();
                ViewData["AkademikPersoneller"] = _akademikPersonelService.GetList();
                
                if (bolumId.HasValue)
                {
                    ViewData["SeciliBolumId"] = bolumId.Value;
                    var bolumSinavlari = _sinavDetayService.GetByBolumId(bolumId.Value);
                    ViewData["BolumSinavlari"] = bolumSinavlari.Data;
                }
                
                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Sayfa yüklenirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet]
        [Route("SinavDetay/GetEvents")]
        public IActionResult GetEvents(int? bolumId)
        {
            try
            {
                IDataResult<List<SinavDetayDTO>> result;
                
                if (bolumId.HasValue)
                {
                    result = _sinavDetayService.GetByBolumId(bolumId.Value);
                }
                else
                {
                    result = _sinavDetayService.GetAllDetails();
                }

                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                var events = result.Data.Select(s => new
                {
                    id = s.Id,
                    title = $"{s.DersAd} - {s.BolumAd}",
                    start = $"{s.SinavTarihi:yyyy-MM-dd}T{s.SinavBaslangicSaati:HH:mm}",
                    end = $"{s.SinavTarihi:yyyy-MM-dd}T{s.SinavBitisSaati:HH:mm}",
                    extendedProps = new
                    {
                        dbapId = s.DersBolumAkademikPersonelId,
                        dersAd = s.DersAd,
                        bolumAd = s.BolumAd,
                        akademikPersonelAd = s.AkademikPersonelAd,
                        derslikler = s.Derslikler ?? new List<DerslikGozetmenDTO>(),
                        gozetmenler = s.Gozetmenler
                    }
                });

                return Ok(events);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Sınav bilgileri alınırken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        [Route("SinavDetay/Add")]
        public IActionResult Add([FromBody]SinavKayitDTO model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new { success = false, message = "Gönderilen veri boş olamaz" });
                }

                var result = _sinavDetayService.Add(model);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Hata: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return BadRequest(new { success = false, message = "Sınav eklenirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPut]
        [Route("SinavDetay/Update")]
        public IActionResult Update([FromBody] SinavGuncelleDTO model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new { success = false, message = "Gönderilen veri boş olamaz" });
                }

                var result = _sinavDetayService.Update(model);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = "Sınav güncellenirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        [Route("SinavDetay/Delete")]
        public IActionResult Delete([FromBody] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { Success = false, Message = "Geçersiz sınav ID'si" });
                }

                // Gelen veriyi logla
                System.Diagnostics.Debug.WriteLine($"Silinecek sınav ID: {id}");

                var result = _sinavDetayService.Delete(new SinavDetay { Id = id });
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Hata: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return BadRequest(new { Success = false, Message = "Sınav silinirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet]
        [Route("SinavDetay/GetById/{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Geçersiz sınav ID'si" });
                }

                var result = _sinavDetayService.GetById(id);
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Sınav bilgisi alınırken bir hata oluştu: " + ex.Message });
            }
        }
    }
}
