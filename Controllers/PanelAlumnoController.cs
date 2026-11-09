using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EnglishSimulator.Data;
using EnglishSimulator.Models.Database;
using Microsoft.AspNetCore.Authorization; // LIBRERIA PARA AUTORIZACION DE USUARIOS

namespace EnglishSimulator.Controllers
{
    [Authorize] // CADA USUARIO DEBE ESTAR LOGUEADO PARA ACCEDER A ESTE CONTROLADOR
    public class PanelAlumnoController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Inyectamos la base de datos al controlador
        public PanelAlumnoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() { return View(); }
        public IActionResult SimuladorPractica() { return View(); }
        public IActionResult PruebaFinal() { return View(); }
        public IActionResult Simulador() { return View(); }
        public IActionResult Resultados() { return View(); }

       //HISTORIAL CON DATOS REALES
        public async Task<IActionResult> Historial()
        {
            // 1. ¿Quién está logueado?
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!uint.TryParse(userIdClaim, out uint userId)) return RedirectToAction("Index", "Acceso");

            // 2. Traer todos sus intentos terminados desde MySQL
            var intentos = await (from i in _context.Set<IntentosExaman>()
                                  join t in _context.Set<TiposPrueba>() on i.IdTipoPrueba equals t.IdTipoPrueba
                                  where i.IdUsuario == userId && i.Resultado != "EN_PROCESO"
                                  orderby i.FechaIntento descending
                                  select new HistorialIntentoDto
                                  {
                                      Fecha = i.FechaIntento,
                                      Tipo = t.NombrePrueba,
                                      Puntaje = i.PorcentajeObtenido ?? 0,
                                      Resultado = i.Resultado
                                  }).ToListAsync();

            // 3. Llenar Tarjetas Superiores
            ViewBag.Practicas = intentos.Count(i => i.Tipo == "Práctica");
            ViewBag.Finales = intentos.Count(i => i.Tipo == "Final");

            var mejorFinal = intentos.Where(i => i.Tipo == "Final" && i.Resultado == "APROBADO")
                                     .Max(i => (decimal?)i.Puntaje) ?? 0;

            string nivel = "SIN NIVEL";
            if (mejorFinal >= 90) nivel = "AVANZADO";
            else if (mejorFinal >= 80) nivel = "INTERMEDIO";
            else if (mejorFinal >= 70) nivel = "BÁSICO";
            ViewBag.Nivel = nivel;

            // 4. Preparar datos para las Gráficas
            var paraGrafica = intentos.OrderBy(i => i.Fecha).ToList(); // Orden cronológico
            ViewBag.Labels = paraGrafica.Select(i => i.Tipo).ToList();
            ViewBag.Scores = paraGrafica.Select(i => i.Puntaje).ToList();

            decimal mejora = 0;
            if (paraGrafica.Count >= 2)
            {
                mejora = paraGrafica.Last().Puntaje - paraGrafica.First().Puntaje;
            }
            ViewBag.Mejora = mejora > 0 ? mejora : 0;
            ViewBag.Intentos = intentos;

            return View();
        }
    }

    // Clase de apoyo para la tabla
    public class HistorialIntentoDto
    {
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }
        public decimal Puntaje { get; set; }
        public string Resultado { get; set; }
    }
}