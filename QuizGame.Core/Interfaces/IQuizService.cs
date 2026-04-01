using QuizGame.Core.Entities;
using QuizGame.Core.Enums;

namespace QuizGame.Core.Interfaces;

public interface IQuizService
{
    Task<Quiz> CreateQuizAsync(string userId, int categoryId, Difficulty difficulty, int questionCount, List<QuestionType> questionTypes, bool isMultiplayer);
    Task<Quiz?> GetQuizAsync(int quizId);
    Task<Quiz> SubmitAnswersAsync(int quizId, string userId, Dictionary<int, string> answers);
}
