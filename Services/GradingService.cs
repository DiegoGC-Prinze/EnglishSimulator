namespace EnglishSimulator.Services;

public class GradingService
{
    public GradingResult Calculate(
        string examType,
        int correctAnswers)
    {
        int totalQuestions;
        decimal pointsPerQuestion;

        switch (examType.ToLowerInvariant())
        {
            case "practice":
                totalQuestions = 20;
                pointsPerQuestion = 5m;
                break;

            case "final":
                totalQuestions = 40;
                pointsPerQuestion = 2.5m;
                break;

            default:
                throw new ArgumentException(
                    "Tipo de examen no válido.");
        }

        if (correctAnswers < 0 ||
            correctAnswers > totalQuestions)
        {
            throw new ArgumentOutOfRangeException(
                nameof(correctAnswers));
        }

        decimal score = correctAnswers * pointsPerQuestion;

        return new GradingResult
        {
            TotalQuestions = totalQuestions,
            CorrectAnswers = correctAnswers,
            Score = score,
            IsPassed = score >= 70m
        };
    }
}

public class GradingResult
{
    public int TotalQuestions { get; set; }

    public int CorrectAnswers { get; set; }

    public decimal Score { get; set; }

    public bool IsPassed { get; set; }
}
