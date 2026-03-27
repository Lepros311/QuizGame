using QuizGame.Core.Entities;
using QuizGame.Core.Interfaces;
using QuizGame.Infrastructure.Data;

namespace QuizGame.Infrastructure.Services;

public class StatBoardService : IStatBoardService
{
    private readonly AppDbContext _context;

    public StatBoardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StatBoard>> GetAllStatBoardsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<StatBoard?> GetStatBoardAsync(int statBoardId)
    {
        throw new NotImplementedException();
    }

    public async Task<UserStatBoard> GetUserStatsAsync(string userId, int statBoardId)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateUserStatsAsync(string userId, int quizId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<UserStatBoard>> GetStatBoardRankingsAsync(int statBoardId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<UserStatBoard>> GetFollowingRankingsAsync(int statBoardId, string userId)
    {
        throw new NotImplementedException();
    }
}
