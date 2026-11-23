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
    ushort TimeSeconds,
    List<ExamOptionDto> Options,
    string? ImageUrl = null
);

public class ExamService
{
    private static readonly (uint LevelId, string LevelName, int Count)[] FinalQuotas =
    {
        (1, "Básico", 13),
        (2, "Intermedio", 13),
        (3, "Avanzado", 14)
    };

    private readonly ApplicationDbContext _context;

    public ExamService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExamQuestionDto>> GenerateExamAsync(
        string examType)
    {
        bool isFinal = examType?.Trim().ToLowerInvariant() switch
        {
            "practice" => false,
            "final" => true,
            _ => throw new ArgumentException(
                "Tipo de examen no válido.")
        };

        // Obtener preguntas reales desde MySQL.
        var questions = await _context.Preguntas
            .AsNoTracking()
            .Select(p => new
            {
                p.IdPregunta,
                p.IdNivel,
                p.TextoPregunta,
                p.SegundosTiempo
            })
            .ToListAsync();

        if (isFinal)
        {
            var questionsByLevel = questions.ToLookup(p => p.IdNivel);
            questions.Clear();

            foreach (var (levelId, levelName, count) in FinalQuotas)
            {
                var levelQuestions = questionsByLevel[levelId].ToList();

                if (levelQuestions.Count < count)
                {
                    throw new InvalidOperationException(
                        $"No hay suficientes preguntas para el examen final en el nivel {levelName}. " +
                        $"Se requieren {count} y hay {levelQuestions.Count} disponibles.");
                }

                Shuffle(levelQuestions);
                questions.AddRange(levelQuestions.Take(count));
            }
        }
        else if (questions.Count < 20)
        {
            throw new InvalidOperationException(
                "No hay suficientes preguntas disponibles.");
        }

        Shuffle(questions);
        var selected = isFinal ? questions : questions.Take(20).ToList();

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

    private static void Shuffle<T>(List<T> questions)
    {
        for (int i = questions.Count - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);

            (questions[i], questions[j]) =
                (questions[j], questions[i]);
        }
    }
}
