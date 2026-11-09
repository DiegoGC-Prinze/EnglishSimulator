using Microsoft.AspNetCore.Mvc;

namespace EnglishSimulator.Controllers
{
    public class AccesoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}