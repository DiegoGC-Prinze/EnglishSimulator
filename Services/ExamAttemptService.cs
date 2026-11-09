using System.Data;
using EnglishSimulator.Data;
using EnglishSimulator.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace EnglishSimulator.Services;

public record ExamSessionDto(
    uint AttemptId,
    string ExamType,
    bool Resumed,
    ushort SecondsRemaining,
    List<uint> QuestionIds
);

public class ExamAttemptService
{
    private readonly ApplicationDbContext _context;
    private readonly ExamService _examService;

    public ExamAttemptService(
        ApplicationDbContext context,
        ExamService examService)
    {
        _context = context;
        _examService = examService;
    }

    public async Task<ExamSessionDto> StartOrResumeAsync(
        uint userId,
        string examType)
    {
        string normalizedType =
            examType?.Trim().ToLowerInvariant()
            ?? string.Empty;

        string databaseType = normalizedType switch
        {
            "practice" => "Práctica",
            "final" => "Final",
            _ => throw new ArgumentException(
                "Tipo de examen no válido.")
        };

        // La transacción evita guardar intentos incompletos.
        // Serializable ayuda a controlar solicitudes simultáneas.
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        // Comprobar que el estudiante exista.
        bool userExists = await _context.Set<Usuario>()
            .AnyAsync(u => u.IdUsuario == userId);

        if (!userExists)
            throw new KeyNotFoundException(
                "El usuario no existe.");

        // Obtener los límites desde MySQL.
        var examTypeData = await _context.Set<TiposPrueba>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                t => t.NombrePrueba == databaseType);

        if (examTypeData is null)
            throw new InvalidOperationException(
                "El tipo de examen no está configurado.");

        // Primero buscar un examen pendiente de ese tipo.
        var existing = await _context.Set<IntentosExaman>()
            .AsNoTracking()
            .Where(i =>
                i.IdUsuario == userId &&
                i.IdTipoPrueba == examTypeData.IdTipoPrueba &&
                i.Resultado == "EN_PROCESO")
            .OrderByDescending(i => i.IdIntento)
            .FirstOrDefaultAsync();

        if (existing is not null)
        {
            var savedQuestionIds =
                await _context.Set<RespuestasIntento>()
                    .AsNoTracking()
                    .Where(r => r.IdIntento == existing.IdIntento)
                    .OrderBy(r => r.OrdenPregunta)
                    .Select(r => r.IdPregunta)
                    .ToListAsync();

            await transaction.CommitAsync();

            return new ExamSessionDto(
                existing.IdIntento,
                normalizedType,
                true,
                existing.SegundosRestantes,
                savedQuestionIds
            );
        }

        // Contar intentos anteriores, incluidos los abandonados.
        int usedAttempts =
            await _context.Set<IntentosExaman>()
                .CountAsync(i =>
                    i.IdUsuario == userId &&
                    i.IdTipoPrueba == examTypeData.IdTipoPrueba);

        if (usedAttempts >= examTypeData.MaxIntentos)
            throw new InvalidOperationException(
                "El usuario agotó sus intentos disponibles.");

        // Obtener una selección aleatoria.
        var questions =
            await _examService.GenerateExamAsync(normalizedType);

        if (questions.Count != examTypeData.CantidadPreguntas)
            throw new InvalidOperationException(
                "La cantidad de preguntas no coincide.");

        // Crear el intento.
        var attempt = new IntentosExaman
        {
            IdUsuario = userId,
            IdTipoPrueba = examTypeData.IdTipoPrueba,
            Resultado = "EN_PROCESO",
            PuntajeRequerido = 70m,
            PorcentajeObtenido = null,
            FechaIntento = DateTime.UtcNow,
            SegundosRestantes = 60,
            UltimaSincronizacion = null
        };

        _context.Set<IntentosExaman>().Add(attempt);
        await _context.SaveChangesAsync();

        // Guardar todas las preguntas y su orden.
        var assignedQuestions = questions
            .Select((q, index) => new RespuestasIntento
            {
                IdIntento = attempt.IdIntento,
                IdPregunta = q.Id,
                OrdenPregunta = checked((byte)(index + 1)),
                IdOpcion = null,
                EsCorrecta = false,
                TiempoRespuesta = null
            })
            .ToList();

        _context.Set<RespuestasIntento>()
            .AddRange(assignedQuestions);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new ExamSessionDto(
            attempt.IdIntento,
            normalizedType,
            false,
            attempt.SegundosRestantes,
            questions.Select(q => q.Id).ToList()
        );
    }
}
