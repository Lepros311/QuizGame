using QuizGame.Core.Enums;

namespace QuizGame.Core.Models;

public class QuizConfig
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Difficulty Difficulty { get; set; }
    public int QuestionCount { get; set; }
    public List<QuestionType> QuestionTypes { get; set; } = [];
    public bool IsMultiplayer { get; set; }
    public bool WasRandomized { get; set; }
}
