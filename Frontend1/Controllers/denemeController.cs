using Business.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Frontend1.Controllers
{
    public class denemeController : Controller
    {
        private readonly ISinavDetayService _sinavDetayService;
        private readonly IDBAPService _dBAPService;

        public denemeController(ISinavDetayService sinavDetayService, IDBAPService dBAPService)
        {
            _sinavDetayService = sinavDetayService;
            _dBAPService = dBAPService;
        }
        public IActionResult Index()
        {
            ViewData["SinavDetaylar"] = _sinavDetayService.GetAll().Data;
            return View();
        }
    }
}
