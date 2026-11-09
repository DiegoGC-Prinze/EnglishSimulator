using Microsoft.AspNetCore.Mvc;
//SE AGREGA PARA PODER CERRAR SESION
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace EnglishSimulator.Controllers
{
    public class AccesoController : Controller
    {
        //Carga la pantalla de Login (GET: /Acceso/Index)
        public IActionResult Index()
        {
            return View();
        }

        // Cierra la sesión (GET: /Acceso/Salir)
        public async Task<IActionResult> Salir()
        {
            // Destruye la cookie de sesión
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirige al login limpio
            return RedirectToAction("Index", "Acceso");
        }
    }
}