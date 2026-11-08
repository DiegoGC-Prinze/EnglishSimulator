using Microsoft.AspNetCore.Mvc;

namespace EnglishSimulator.Controllers
{
    public class PanelAlumnoController : Controller
    {
        // GET: /PanelAlumno/Index (Tu vista actual del Dashboard)
        public IActionResult Index()
        {
            return View();
        }

        // GET: /PanelAlumno/SimuladorPractica
        public IActionResult SimuladorPractica()
        {
            return View();
        }

        // GET: /PanelAlumno/PruebaFinal
        public IActionResult PruebaFinal()
        {
            return View();
        }
        //GET : /PanelAlumno/Simulador
        public IActionResult Simulador() 
        { 
            return View(); 
        }
        // GET: /PanelAlumno/Resultados
        public IActionResult Resultados()
        {
            return View();
        }
        // GET: /PanelAlumno/Historial
        public IActionResult Historial()
        {
            return View();
        }
    }
}