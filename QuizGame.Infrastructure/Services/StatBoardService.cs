using Microsoft.EntityFrameworkCore;
using QuizGame.Core;
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
        return await _context.StatBoards.ToListAsync();
    }

    public async Task<StatBoard?> GetStatBoardAsync(int statBoardId)
    {
        return await _context.StatBoards.FirstOrDefaultAsync(s => s.Id == statBoardId);
    }

    public async Task<UserStatBoard> GetUserStatsAsync(string userId)
    {
        var stats = await _context.UserStatBoards.FirstOrDefaultAsync(u => u.UserId == userId);

        if (stats == null)
        {
            throw new ArgumentException("User stats not found.", nameof(userId));
        }

        return stats;
    }

    public async Task UpdateUserStatsAsync(string userId, int quizId)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .Include(q => q.Category)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz == null)
        {
            throw new ArgumentException("Quiz not found.", nameof(quizId));
        }

        var stats = await _context.UserStatBoards
            .FirstOrDefaultAsync(u => u.UserId == userId);

        // Create stats record if it doesn't exist yet
        if (stats == null)
        {
            stats = new UserStatBoard { UserId = userId };
            _context.UserStatBoards.Add(stats);
        }

        // Update performance stats
        stats.TotalQuizzesCompleted++;
        stats.TotalCorrectAnswers += quiz.Questions.Count(q => q.IsCorrect == true);
        stats.TotalWrongAnswers += quiz.Questions.Count(q => q.IsCorrect == false);

        var totalAnswered = stats.TotalCorrectAnswers + stats.TotalWrongAnswers;
        stats.AverageScorePercentage = totalAnswered > 0
            ? (double)stats.TotalCorrectAnswers / totalAnswered * 100
            : 0;

        if (quiz.Score > stats.HighestScore)
        {
            stats.HighestScore = quiz.Score;
        }

        // Update speed stats
        if (quiz.StartedAt.HasValue && quiz.CompletedAt.HasValue)
        {
            var completionSeconds = (quiz.CompletedAt.Value - quiz.StartedAt.Value).TotalSeconds;

            if (stats.FastestCompletionSeconds == 0 || completionSeconds < stats.FastestCompletionSeconds)
            {
                stats.FastestCompletionSeconds = completionSeconds;
            }

            stats.AverageCompletionSeconds = stats.AverageCompletionSeconds == 0
                ? completionSeconds
                : (stats.AverageCompletionSeconds + completionSeconds) / 2;
        }

        // Update category stats
        if (quiz.Category != null)
        {
            stats.MostPlayedCategory = quiz.Category.Name;
        }

        // Update streak stats
        var scorePercentage = quiz.QuestionCount > 0
            ? (double)quiz.Score / quiz.QuestionCount * 100
            : 0;

        if (scorePercentage >= 50)
        {
            stats.CurrentWinStreak++;
            if (stats.CurrentWinStreak > stats.LongestWinStreak)
            {
                stats.LongestWinStreak = stats.CurrentWinStreak;
            }
        }
        else
        {
            stats.CurrentWinStreak = 0;
        }

        stats.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserStatBoard>> GetGlobalRankingsAsync(int statBoardId)
    {
        var statBoard = await _context.StatBoards.FirstOrDefaultAsync(s => s.Id == statBoardId);

        if (statBoard == null)
        {
            throw new ArgumentException("Stat board not found.", nameof(statBoardId));
        }

        var query = _context.UserStatBoards.AsQueryable();

        return statBoard.Name switch
        {
            StatBoardConstants.TopScores => await query
                .OrderByDescending(u => u.HighestScore)
                .ToListAsync(),
            StatBoardConstants.WinStreaks => await query
                .OrderByDescending(u => u.LongestWinStreak)
                .ToListAsync(),
            StatBoardConstants.FastestCompletions => await query
                .OrderBy(u => u.FastestCompletionSeconds)
                .ToListAsync(),
            StatBoardConstants.MostChallengesWon => await query
                .OrderByDescending(u => u.TotalChallengesWon)
                .ToListAsync(),
            StatBoardConstants.MostQuizzesCompleted => await query
                .OrderByDescending(u => u.TotalQuizzesCompleted)
                .ToListAsync(),
            _ => await query.ToListAsync()
        };
    }

    public async Task<IEnumerable<UserStatBoard>> GetFollowingRankingsAsync(int statBoardId, string userId)
    {
        var followingIds = await _context.UserFollows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        var statBoard = await _context.StatBoards.FirstOrDefaultAsync(s => s.Id == statBoardId);

        if (statBoard == null)
        {
            throw new ArgumentException("StatBoard not found.", nameof(statBoardId));
        }

        var query = _context.UserStatBoards
            .Where(u => followingIds.Contains(u.UserId))
            .AsQueryable();

        return statBoard.Name switch
        {
            StatBoardConstants.TopScores => await query
                .OrderByDescending(u => u.HighestScore)
                .ToListAsync(),
            StatBoardConstants.WinStreaks => await query
                .OrderByDescending(u => u.LongestWinStreak)
                .ToListAsync(),
            StatBoardConstants.FastestCompletions => await query
                .OrderBy(u => u.FastestCompletionSeconds)
                .ToListAsync(),
            StatBoardConstants.MostChallengesWon => await query
                .OrderByDescending(u => u.TotalChallengesWon)
                .ToListAsync(),
            StatBoardConstants.MostQuizzesCompleted => await query
                .OrderByDescending(u => u.TotalQuizzesCompleted)
                .ToListAsync(),
            _ => await query.ToListAsync()
        };
    }
}
