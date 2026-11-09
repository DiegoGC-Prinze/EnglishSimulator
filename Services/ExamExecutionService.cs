using EnglishSimulator.Data;
using EnglishSimulator.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace EnglishSimulator.Services;

public record ExamStateDto(
    uint AttemptId,
    string Status,
    int TotalQuestions,
    int CompletedQuestions,
    byte? CurrentPosition,
    ushort SecondsRemaining,
    ExamQuestionDto? CurrentQuestion,
    decimal? Score
);

public class ExamExecutionService
{
    private const ushort QuestionLimit = 60;
    private const int MaximumHeartbeatGap = 8;

    private readonly ApplicationDbContext _context;
    private readonly GradingService _grading;

    public ExamExecutionService(
        ApplicationDbContext context,
        GradingService grading)
    {
        _context = context;
        _grading = grading;
    }

    // Recuperar el intento y comprobar su propietario.

private async Task<(
    IntentosExaman Attempt,
    List<RespuestasIntento> Answers)> LoadAsync(
    uint userId,
    uint attemptId)
{
    // Bloquea el intento durante la transacción actual
    // para evitar modificaciones simultáneas.
    var attempts = await _context.Set<IntentosExaman>()
        .FromSqlInterpolated($"""
            SELECT *
            FROM intentos_examen
            WHERE id_intento = {attemptId}
              AND id_usuario = {userId}
            FOR UPDATE
            """)
        .ToListAsync();

    var attempt = attempts.SingleOrDefault();

    if (attempt is null)
        throw new KeyNotFoundException(
            "El intento no existe.");

    var answers = await _context
        .Set<RespuestasIntento>()
        .Where(r => r.IdIntento == attemptId)
        .OrderBy(r => r.OrdenPregunta)
        .ToListAsync();

    return (attempt, answers);
}


    // Descontar tiempo únicamente mientras el
    // frontend se encuentre sincronizando.
    private static void UpdateClock(
        IntentosExaman attempt,
        List<RespuestasIntento> answers,
        DateTime now)
    {
        if (attempt.Resultado != "EN_PROCESO" ||
            attempt.UltimaSincronizacion is null)
            return;

        var current = answers.FirstOrDefault(
            r => r.TiempoRespuesta is null);

        if (current is null)
            return;

        double difference =
            (now - attempt.UltimaSincronizacion.Value)
            .TotalSeconds;

        if (difference < 1)
            return;

        // Si el navegador desapareció, no descontamos
        // todo el tiempo transcurrido fuera de la app.
        int elapsed = (int)Math.Min(
            Math.Floor(difference),
            MaximumHeartbeatGap);

        attempt.SegundosRestantes = (ushort)Math.Max(
            0,
            attempt.SegundosRestantes - elapsed);

        if (attempt.SegundosRestantes == 0)
        {
            // Pregunta agotada y sin respuesta.
            current.IdOpcion = null;
            current.EsCorrecta = false;
            current.TiempoRespuesta = QuestionLimit;

            bool hasNext = answers.Any(
                r => r.TiempoRespuesta is null);

            attempt.SegundosRestantes =
                hasNext ? QuestionLimit : (ushort)0;

            // La siguiente pregunta se activará
            // cuando el cliente solicite reanudar.
            attempt.UltimaSincronizacion = null;
            return;
        }

        // Una interrupción prolongada pausa el intento.
        attempt.UltimaSincronizacion =
            difference > MaximumHeartbeatGap
                ? null
                : now;
    }

    // Reanudar o sincronizar el temporizador.
    public async Task<ExamStateDto> SyncAsync(
        uint userId,
        uint attemptId,
        bool resume)
    {
        var (attempt, answers) =
            await LoadAsync(userId, attemptId);

        var now = DateTime.UtcNow;

        UpdateClock(attempt, answers, now);

        if (attempt.Resultado == "EN_PROCESO")
        {
            if (answers.All(r => r.TiempoRespuesta != null))
            {
                await CompleteAsync(attempt, answers);
            }
            else if (resume &&
                     attempt.UltimaSincronizacion is null)
            {
                attempt.UltimaSincronizacion = now;
            }
        }

        await _context.SaveChangesAsync();

        return await BuildStateAsync(attempt, answers);
    }

    // Pausar y conservar el tiempo restante.
    public async Task<ExamStateDto> PauseAsync(
        uint userId,
        uint attemptId)
    {
        var (attempt, answers) =
            await LoadAsync(userId, attemptId);

        UpdateClock(attempt, answers, DateTime.UtcNow);

        attempt.UltimaSincronizacion = null;

        if (attempt.Resultado == "EN_PROCESO" &&
            answers.All(r => r.TiempoRespuesta != null))
        {
            await CompleteAsync(attempt, answers);
        }

        await _context.SaveChangesAsync();

        return await BuildStateAsync(attempt, answers);
    }

    // Registrar una respuesta.
    public async Task<ExamStateDto> AnswerAsync(
        uint userId,
        uint attemptId,
        uint questionId,
        uint optionId)
    {
        var (attempt, answers) =
            await LoadAsync(userId, attemptId);

        if (attempt.Resultado != "EN_PROCESO")
            throw new InvalidOperationException(
                "Este examen ya terminó.");

        var now = DateTime.UtcNow;

        UpdateClock(attempt, answers, now);


var current = answers.FirstOrDefault(
    r => r.TiempoRespuesta is null);

if (current is null)
{
    await CompleteAsync(attempt, answers);
    await _context.SaveChangesAsync();

    return await BuildStateAsync(attempt, answers);
}

// Si la pregunta venció, conservar el avance
// y devolver la pregunta actualizada al frontend.
if (current.IdPregunta != questionId)
{
    await _context.SaveChangesAsync();

    return await BuildStateAsync(attempt, answers);
}

// Si la sesión quedó pausada, guardar su estado.
// El frontend deberá llamar a /resume.
if (attempt.UltimaSincronizacion is null)
{
    await _context.SaveChangesAsync();

    return await BuildStateAsync(attempt, answers);
}
;

        // La respuesta correcta se consulta desde MySQL.
        // Nunca se confía en un valor enviado por el cliente.
        var option = await _context.Set<Opcione>()
            .AsNoTracking()
            .SingleOrDefaultAsync(o =>
                o.IdOpcion == optionId &&
                o.IdPregunta == questionId);

        if (option is null)
            throw new ArgumentException(
                "La opción seleccionada no pertenece a la pregunta.");

        current.IdOpcion = optionId;
        current.EsCorrecta = option.EsCorrecta;
        current.TiempoRespuesta = (ushort)(
            QuestionLimit - attempt.SegundosRestantes);

        bool hasNext = answers.Any(
            r => r.TiempoRespuesta is null);

        if (hasNext)
        {
            attempt.SegundosRestantes = QuestionLimit;
            attempt.UltimaSincronizacion = now;
        }
        else
        {
            await CompleteAsync(attempt, answers);
        }

        await _context.SaveChangesAsync();

        return await BuildStateAsync(attempt, answers);
    }

    // Finalizar voluntariamente, incluso con
    // preguntas sin contestar.
    public async Task<ExamStateDto> FinishAsync(
        uint userId,
        uint attemptId)
    {
        var (attempt, answers) =
            await LoadAsync(userId, attemptId);

        if (attempt.Resultado == "EN_PROCESO")
        {
            UpdateClock(attempt, answers, DateTime.UtcNow);

            var current = answers.FirstOrDefault(
                r => r.TiempoRespuesta is null);

            if (current is not null)
            {
                current.TiempoRespuesta = (ushort)(
                    QuestionLimit - attempt.SegundosRestantes);
            }

            foreach (var answer in answers.Where(
                r => r.TiempoRespuesta is null))
            {
                answer.TiempoRespuesta = 0;
            }

            await CompleteAsync(attempt, answers);
            await _context.SaveChangesAsync();
        }

        return await BuildStateAsync(attempt, answers);
    }

    // Calcular el resultado final con GradingService.
    private async Task CompleteAsync(
        IntentosExaman attempt,
        List<RespuestasIntento> answers)
    {
        var type = await _context.Set<TiposPrueba>()
            .AsNoTracking()
            .SingleAsync(t =>
                t.IdTipoPrueba == attempt.IdTipoPrueba);

        string examType = type.NombrePrueba == "Práctica"
            ? "practice"
            : "final";

        int correct = answers.Count(r => r.EsCorrecta);

        var result = _grading.Calculate(
            examType, correct);

        attempt.PorcentajeObtenido = result.Score;
        attempt.Resultado = result.IsPassed
            ? "APROBADO"
            : "REPROBADO";

        attempt.SegundosRestantes = 0;
        attempt.UltimaSincronizacion = null;
    }

    // Construir información segura para el frontend.
    private async Task<ExamStateDto> BuildStateAsync(
        IntentosExaman attempt,
        List<RespuestasIntento> answers)
    {
        var current = attempt.Resultado == "EN_PROCESO"
            ? answers.FirstOrDefault(
                r => r.TiempoRespuesta is null)
            : null;

        ExamQuestionDto? questionDto = null;

        if (current is not null)
        {
            var question = await _context.Set<Pregunta>()
                .AsNoTracking()
                .SingleAsync(p =>
                    p.IdPregunta == current.IdPregunta);

            var options = await _context.Set<Opcione>()
                .AsNoTracking()
                .Where(o => o.IdPregunta == current.IdPregunta)
                .Select(o => new ExamOptionDto(
                    o.IdOpcion,
                    o.TextoOpcion))
                .ToListAsync();

            questionDto = new ExamQuestionDto(
                question.IdPregunta,
                question.TextoPregunta,
                question.SegundosTiempo,
                options);
        }

        return new ExamStateDto(
            attempt.IdIntento,
            attempt.Resultado,
            answers.Count,
            answers.Count(r => r.TiempoRespuesta != null),
            current?.OrdenPregunta,
            attempt.SegundosRestantes,
            questionDto,
            attempt.PorcentajeObtenido
        );
    }
}
