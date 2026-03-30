namespace QuizGame.Core.Models.Requests;

public class SubmitAnswersRequest
{
    public Dictionary<int, string> Answers { get; set; } = [];
}
