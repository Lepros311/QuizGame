using QuizGame.Core.Enums;

namespace QuizGame.Core.Models.DTOs;

public class QuizDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public CategoryDto? Category { get; set; }
    public Difficulty Difficulty { get; set; }
    public int QuestionCount { get; set; }
    public List<QuestionType> QuestionTypes { get; set; } = [];
    public bool IsMultiplayer { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int Score { get; set; }
    public List<QuestionDto> Questions { get; set; } = [];
}
