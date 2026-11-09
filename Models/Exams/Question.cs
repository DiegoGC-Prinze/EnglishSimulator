namespace EnglishSimulator.Models.Exams;

public class Question
{
    public int Id { get; set;}
    public string Text{ get; set;} = string.Empty;

    public string Level {get; set;} = string.Empty;

    public List<AnswerOption> Options { get; set; } = new();
}