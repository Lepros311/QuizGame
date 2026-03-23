using QuizGame.Core.Enums;

namespace QuizGame.Core.Entities;

public class Quiz
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public Difficulty Difficulty { get; set; }
    public int QuestionCount { get; set; }
    public List<QuestionType> QuestionTypes { get; set; } = [];
    public bool IsMultiplayer { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Question> Questions { get; set; } = [];
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
