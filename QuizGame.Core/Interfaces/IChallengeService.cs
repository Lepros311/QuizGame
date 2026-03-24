using QuizGame.Core.Entities;
using QuizGame.Core.Enums;

namespace QuizGame.Core.Interfaces;

public interface IChallengeService
{
    Task<Challenge> CreateChallengeAsync(
            string challengerId,
            int categoryId,
            Difficulty difficulty,
            int questionCount,
            List<QuestionType> questionTypes,
            List<string> opponentIds);
    Task<Challenge?> GetChallengeAsync(int challengeId);
    Task<IEnumerable<Challenge>> GetUserChallengesAsync(string userId);
    Task<Challenge> AcceptChallengeAsync(int challengeId, string userId);
    Task<Challenge> DeclineChallengeAsync(int challengeId, string userId);
    Task<Challenge> SubmitChallengeAnswersAsync(int challengeId, string userId, Dictionary<int, string> answers);
}
