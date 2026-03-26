using QuizGame.Core.Enums;
using QuizGame.Core.Models;

namespace QuizGame.Core.Interfaces;

public interface IQuizConfigurationService
{
    Task<QuizConfig> ResolveConfigAsync(
        int? categoryId,
        Difficulty? difficulty,
        int? questionCount,
        List<QuestionType>? questionTypes,
        bool isMultiplayer);
}
