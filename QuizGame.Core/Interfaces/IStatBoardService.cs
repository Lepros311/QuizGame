using QuizGame.Core.Entities;

namespace QuizGame.Core.Interfaces;

public interface IStatBoardService
{
    Task<IEnumerable<StatBoard>> GetAllStatBoardsAsync();
    Task<StatBoard?> GetStatBoardAsync(int statBoardId);
    Task<UserStatBoard> GetUserStatsAsync(string userId);
    Task UpdateUserStatsAsync(string userId, int quizId);
    Task<IEnumerable<UserStatBoard>> GetGlobalRankingsAsync(int statBoardId);
    Task<IEnumerable<UserStatBoard>> GetFollowingRankingsAsync(int statBoardId, string userId);
}
