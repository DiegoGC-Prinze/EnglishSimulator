using System.Security.Claims;
using EnglishSimulator.Data;
using EnglishSimulator.Models.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnglishSimulator.Controllers;

[ApiController]
[Authorize]
[Route("api/results")]
public class ApiResultsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ApiResultsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private uint? GetUserId()
    {
        string? value = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return uint.TryParse(value, out uint id)
            ? id
            : null;
    }

    // Estadísticas generales del estudiante.
    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var types = await _context.Set<TiposPrueba>()
            .AsNoTracking()
            .OrderBy(t => t.IdTipoPrueba)
            .ToListAsync();

        var attempts = await _context.Set<IntentosExaman>()
            .AsNoTracking()
            .Where(i => i.IdUsuario == userId.Value)
            .ToListAsync();

        var statistics = types.Select(type =>
        {
            var all = attempts
                .Where(i => i.IdTipoPrueba == type.IdTipoPrueba)
                .ToList();

            var completed = all
                .Where(i =>
                    (i.Resultado == "APROBADO" ||
                     i.Resultado == "REPROBADO") &&
                    i.PorcentajeObtenido.HasValue)
                .ToList();

            decimal? average = completed.Count == 0
                ? null
                : Math.Round(
                    completed.Average(i =>
                        i.PorcentajeObtenido!.Value), 2);

            decimal? best = completed.Count == 0
                ? null
                : completed.Max(i =>
                    i.PorcentajeObtenido!.Value);

            return new
            {
                typeId = type.IdTipoPrueba,
                examType = type.NombrePrueba,
                totalQuestions = type.CantidadPreguntas,
                maxAttempts = type.MaxIntentos,
                attemptsUsed = all.Count,
                attemptsRemaining = Math.Max(
                    0, (int)type.MaxIntentos - all.Count),
                completedAttempts = completed.Count,
                passedAttempts = completed.Count(i =>
                    i.Resultado == "APROBADO"),
                inProgress = all.Any(i =>
                    i.Resultado == "EN_PROCESO"),
                averageScore = average,
                bestScore = best
            };
        }).ToList();

        return Ok(new
        {
            totalAttempts = attempts.Count,
            completedAttempts = attempts.Count(i =>
                i.Resultado == "APROBADO" ||
                i.Resultado == "REPROBADO"),
            passedAttempts = attempts.Count(i =>
                i.Resultado == "APROBADO"),
            statistics
        });
    }

    // Historial de prácticas y exámenes finales.
    [HttpGet("history")]
    public async Task<IActionResult> History()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var attempts = await _context.Set<IntentosExaman>()
            .AsNoTracking()
            .Include(i => i.IdTipoPruebaNavigation)
            .Include(i => i.RespuestasIntentos)
            .Where(i => i.IdUsuario == userId.Value)
            .OrderByDescending(i => i.FechaIntento)
            .ThenByDescending(i => i.IdIntento)
            .ToListAsync();

        var history = attempts.Select(i => new
        {
            attemptId = i.IdIntento,
            examType = i.IdTipoPruebaNavigation.NombrePrueba,
            date = i.FechaIntento,
            status = i.Resultado,
            score = i.PorcentajeObtenido,
            requiredScore = i.PuntajeRequerido,
            totalQuestions = i.RespuestasIntentos.Count,
            answeredQuestions = i.RespuestasIntentos.Count(
                r => r.IdOpcion.HasValue),
            correctAnswers = i.Resultado == "EN_PROCESO"
                ? (int?)null
                : i.RespuestasIntentos.Count(
                    r => r.EsCorrecta)
        });

        return Ok(history);
    }

    // Detalle de un examen terminado.
    [HttpGet("{attemptId}")]
    public async Task<IActionResult> Detail(uint attemptId)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var attempt = await _context.Set<IntentosExaman>()
            .AsNoTracking()
            .Include(i => i.IdTipoPruebaNavigation)
            .Include(i => i.RespuestasIntentos)
            .SingleOrDefaultAsync(i =>
                i.IdIntento == attemptId &&
                i.IdUsuario == userId.Value);

        if (attempt is null)
            return NotFound(new
            {
                message = "El intento no existe."
            });

        if (attempt.Resultado == "EN_PROCESO")
            return Conflict(new
            {
                message = "El examen todavía está en progreso."
            });

        var answers = attempt.RespuestasIntentos
            .OrderBy(r => r.OrdenPregunta)
            .Select(r => new
            {
                position = r.OrdenPregunta,
                questionId = r.IdPregunta,
                selectedOptionId = r.IdOpcion,
                isCorrect = r.EsCorrecta,
                timeSpentSeconds = r.TiempoRespuesta
            })
            .ToList();

        return Ok(new
        {
            attemptId = attempt.IdIntento,
            examType = attempt.IdTipoPruebaNavigation.NombrePrueba,
            date = attempt.FechaIntento,
            status = attempt.Resultado,
            score = attempt.PorcentajeObtenido,
            requiredScore = attempt.PuntajeRequerido,
            totalQuestions = answers.Count,
            correctAnswers = answers.Count(a => a.isCorrect),
            answers
        });
    }
}
