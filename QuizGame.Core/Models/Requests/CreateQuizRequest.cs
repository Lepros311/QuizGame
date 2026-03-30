using QuizGame.Core.Enums;

namespace QuizGame.Core.Models.Requests;

public class CreateQuizRequest
{
    public int? CategoryId { get; set; }
    public Difficulty? Difficulty { get; set; }
    public int? QuestionCount { get; set; }
    public List<QuestionType>? QuestionTypes { get; set; }
    public bool IsMultiplayer { get; set; }
    public List<string>? OpponentIds { get; set; }
}
