using QuizGame.Core.Enums;

namespace QuizGame.Core.Entities;

public class Question
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public List<string> Options { get; set; } = [];
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? UserAnswer { get; set; }
    public bool? IsCorrect { get; set; }
}
