using QuizGame.Core.Enums;

namespace QuizGame.Core.Models.DTOs;

public class QuestionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public List<string> Options { get; set; } = [];
    public string? UserAnswer { get; set; }
    public bool? IsCorrect { get; set; }
    /// <summary>Populated after grading for review (e.g. results screen).</summary>
    public string? CorrectAnswer { get; set; }
}
