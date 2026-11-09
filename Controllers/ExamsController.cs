using System.Data;
using EnglishSimulator.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EnglishSimulator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishSimulator.Controllers;

public record StartExamRequest(string ExamType);

public record SubmitAnswerRequest(
    uint QuestionId,
    uint OptionId
);

[ApiController]
[Authorize]
[AutoValidateAntiforgeryToken]
[Route("api/exams")]
public class ExamsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
private readonly ExamAttemptService _attemptService;
private readonly ExamExecutionService _executionService;

public ExamsController(
    ApplicationDbContext context,
    ExamAttemptService attemptService,
    ExamExecutionService executionService)
{
    _context = context;
    _attemptService = attemptService;
    _executionService = executionService;
}

    // Obtener el ID del usuario autenticado.
    // No aceptamos IDs de usuario proporcionados
    // por el navegador.
    private uint? GetAuthenticatedUserId()
    {
        string? value = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return uint.TryParse(value, out uint userId)
            ? userId
            : null;
    }

    // Respuestas HTTP consistentes para los servicios.

private async Task<IActionResult> ExecuteAsync<T>(
    Func<uint, Task<T>> action,
    bool managesOwnTransaction = false)
{
    var userId = GetAuthenticatedUserId();

    if (userId is null)
    {
        return Unauthorized(new
        {
            message = "Usuario no autenticado."
        });
    }

    try
    {
        // StartOrResumeAsync ya administra su propia transacción.
        if (managesOwnTransaction)
        {
            return Ok(await action(userId.Value));
        }

        // Las operaciones sobre un intento se ejecutan
        // dentro de una misma transacción.
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted);

        var result = await action(userId.Value);

        await transaction.CommitAsync();

        return Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(new
        {
            message = ex.Message
        });
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new
        {
            message = ex.Message
        });
    }
    catch (InvalidOperationException ex)
    {
        return Conflict(new
        {
            message = ex.Message
        });
    }
}


    [HttpPost("start")]
public Task<IActionResult> Start(
    [FromBody] StartExamRequest request)
{
    return ExecuteAsync(
        userId => _attemptService.StartOrResumeAsync(
            userId,
            request.ExamType),
        managesOwnTransaction: true);
}

    [HttpGet("{attemptId}/state")]
    public Task<IActionResult> State(uint attemptId)
    {
        return ExecuteAsync(userId =>
            _executionService.SyncAsync(
                userId,
                attemptId,
                false));
    }

    [HttpPost("{attemptId}/resume")]
    public Task<IActionResult> Resume(uint attemptId)
    {
        return ExecuteAsync(userId =>
            _executionService.SyncAsync(
                userId,
                attemptId,
                true));
    }

    [HttpPost("{attemptId}/heartbeat")]
    public Task<IActionResult> Heartbeat(uint attemptId)
    {
        return ExecuteAsync(userId =>
            _executionService.SyncAsync(
                userId,
                attemptId,
                false));
    }

    [HttpPost("{attemptId}/pause")]
    public Task<IActionResult> Pause(uint attemptId)
    {
        return ExecuteAsync(userId =>
            _executionService.PauseAsync(
                userId,
                attemptId));
    }

    [HttpPost("{attemptId}/answer")]
    public Task<IActionResult> Answer(
        uint attemptId,
        [FromBody] SubmitAnswerRequest request)
    {
        return ExecuteAsync(userId =>
            _executionService.AnswerAsync(
                userId,
                attemptId,
                request.QuestionId,
                request.OptionId));
    }

    [HttpPost("{attemptId}/finish")]
    public Task<IActionResult> Finish(uint attemptId)
    {
        return ExecuteAsync(userId =>
            _executionService.FinishAsync(
                userId,
                attemptId));
    }
}
