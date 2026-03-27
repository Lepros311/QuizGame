using QuizGame.Core.Entities;

namespace QuizGame.Core.Interfaces;

public interface IStatBoardService
{
    Task<IEnumerable<StatBoard>> GetAllStatBoardsAsync();
    Task<StatBoard?> GetStatBoardAsync(int statBoardId);
    Task<UserStatBoard> GetUserStatsAsync(string userId, int statBoardId);
    Task UpdateUserStatsAsync(string userId, int quizId);
    Task<IEnumerable<UserStatBoard>> GetStatBoardRankingsAsync(int statBoardId);
    Task<IEnumerable<UserStatBoard>> GetFollowingRankingsAsync(int statBoardId, string userId);
}
