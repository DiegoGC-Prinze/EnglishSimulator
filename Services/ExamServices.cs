using System.Security.Cryptography;
using EnglishSimulator.Data;
using Microsoft.EntityFrameworkCore;

namespace EnglishSimulator.Services;

// Información que se puede enviar al frontend..

public record ExamOptionDto(
    uint Id,
    string Text
);

public record ExamQuestionDto(
    uint Id,
    string Text,
    int TimeLimitSeconds,
    List<ExamOptionDto> Options
);


public class ExamService
{
    private readonly ApplicationDbContext _context;

    public ExamService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExamQuestionDto>> GenerateExamAsync(
        string examType)
    {
        int questionCount = examType?.Trim().ToLowerInvariant() switch
        {
            "practice" => 20,
            "final" => 40,
            _ => throw new ArgumentException(
                "Tipo de examen no válido.")
        };

        // Obtener preguntas reales desde MySQL.
        var questions = await _context.Preguntas
            .AsNoTracking()
            .Select(p => new
            {
                p.IdPregunta,
                p.TextoPregunta,
                p.SegundosTiempo
            })
            .ToListAsync();

        if (questions.Count < questionCount)
        {
            throw new InvalidOperationException(
                "No hay suficientes preguntas disponibles.");
        }

        // Mezclar las preguntas con Fisher-Yates.
        for (int i = questions.Count - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);

            (questions[i], questions[j]) =
                (questions[j], questions[i]);
        }

        // Seleccionar 20 o 40 preguntas sin repetir.
        var selected = questions.Take(questionCount).ToList();

        var questionIds = selected
            .Select(p => p.IdPregunta)
            .ToArray();

        // Consultar las opciones de las preguntas seleccionadas.
        // No consultamos EsCorrecta para enviarla al cliente.
        var options = await _context.Opciones
            .AsNoTracking()
            .Where(o => questionIds.Contains(o.IdPregunta))
            .Select(o => new
            {
                o.IdOpcion,
                o.IdPregunta,
                o.TextoOpcion
            })
            .ToListAsync();

        var optionsByQuestion = options
            .GroupBy(o => o.IdPregunta)
            .ToDictionary(
                g => g.Key,
                g => g.Select(o => new ExamOptionDto(
                    o.IdOpcion,
                    o.TextoOpcion
                )).ToList()
            );

        // Construir el examen para el frontend.
        var exam = selected.Select(p =>
        {
            if (!optionsByQuestion.TryGetValue(
                p.IdPregunta, out var questionOptions)
                || questionOptions.Count < 3)
            {
                throw new InvalidOperationException(
                    $"La pregunta {p.IdPregunta} no tiene opciones válidas.");
            }

            return new ExamQuestionDto(
                p.IdPregunta,
                p.TextoPregunta,
                p.SegundosTiempo,
                questionOptions
            );
        }).ToList();

        return exam;
    }
}
