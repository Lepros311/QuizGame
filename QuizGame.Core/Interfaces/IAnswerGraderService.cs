using QuizGame.Core.Entities;

namespace QuizGame.Core.Interfaces;

public interface IAnswerGraderService
{
    Task<bool> GradeShortAnswerAsync(Question question, string userAnswer);
}
