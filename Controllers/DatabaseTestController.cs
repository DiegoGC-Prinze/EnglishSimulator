using EnglishSimulator.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnglishSimulator.Controllers;

[Route("api/database-test")]
public class DatabaseTestController : Controller
{
    private readonly ApplicationDbContext _context;

    public DatabaseTestController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            bool connected = await _context.Database.CanConnectAsync();

            if (!connected)
            {
                return StatusCode(503, new
                {
                    message = "No se pudo conectar a MySQL."
                });
            }

            var levels = await _context.Niveles
                .AsNoTracking()
                .ToListAsync();

            return Ok(new
            {
                message = "Conexion exitosa con MySQL.",
                totalLevels = levels.Count,
                levels
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                message = "Error al consultar la base de datos."
            });
        }
    }
}
