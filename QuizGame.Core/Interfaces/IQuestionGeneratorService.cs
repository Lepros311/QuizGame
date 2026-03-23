using QuizGame.Core.Entities;
using QuizGame.Core.Enums;

namespace QuizGame.Core.Interfaces;

public interface IQuestionGeneratorService
{
    Task<List<Question>> GenerateQuestionsAsync(
        string categoryName,
        Difficulty difficulty,
        int questionCount,
        List<QuestionType> questionTypes);
}