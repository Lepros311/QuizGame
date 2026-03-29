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
        var stats = await _context.UserStatBoards
            .FirstOrDefaultAsync(u => u.UserId == userId);

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

        var stats = await GetOrCreateUserStatBoardAsync(userId);
        var scorePercentage = CalculateScorePercentage(quiz.Score, quiz.QuestionCount);

        UpdatePerformanceStats(stats, quiz, scorePercentage);
        UpdateSpeedStats(stats, quiz);
        UpdateCategoryStats(stats, quiz);
        UpdateStreakStats(stats, scorePercentage);

        await UpdateDifficultyStatsAsync(userId, quiz, scorePercentage);

        stats.SkillScore = CalculateSkillScore(stats);

        stats.LastUpdated = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserStatBoard>> GetGlobalRankingsAsync(int statBoardId)
    {
        var statBoard = await _context.StatBoards.FirstOrDefaultAsync(s => s.Id == statBoardId);

        if (statBoard == null)
        {
            throw new ArgumentException("StatBoard not found.", nameof(statBoardId));
        }

        return await GetRankingsQueryAsync(_context.UserStatBoards.AsQueryable(), statBoard.Name);
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

        return await GetRankingsQueryAsync(query, statBoard.Name);
    }

    // *** PRIVATE HELPER METHODS ***

    private async Task<UserStatBoard> GetOrCreateUserStatBoardAsync(string userId)
    {
        var stats = await _context.UserStatBoards
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (stats == null)
        {
            stats = new UserStatBoard { UserId = userId };
            _context.UserStatBoards.Add(stats);
        }

        return stats;
    }

    private static double CalculateScorePercentage(int score, int questionCount)
    {
        return questionCount > 0 ? (double)score / questionCount * 100 : 0;
    }

    private static void UpdatePerformanceStats(UserStatBoard stats, Quiz quiz, double scorePercentage)
    {
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
    }

    private static void UpdateSpeedStats(UserStatBoard stats, Quiz quiz)
    {
        if (!quiz.StartedAt.HasValue || !quiz.CompletedAt.HasValue)
        {
            return;
        }

        var completionSeconds = (quiz.CompletedAt.Value - quiz.StartedAt.Value).TotalSeconds;

        if (stats.FastestCompletionSeconds == 0 || completionSeconds < stats.FastestCompletionSeconds)
        {
            stats.FastestCompletionSeconds = completionSeconds;
        }

        stats.AverageCompletionSeconds = stats.AverageCompletionSeconds == 0
            ? completionSeconds
            : (stats.AverageCompletionSeconds + completionSeconds) / 2;
    }

    private static void UpdateCategoryStats(UserStatBoard stats, Quiz quiz)
    {
        if (quiz.Category != null)
        {
            stats.MostPlayedCategory = quiz.Category.Name;
        }
    }

    private static void UpdateStreakStats(UserStatBoard stats, double scorePercentage)
    {
        if (scorePercentage >= GameConstants.WinThresholdPercentage)
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
    }

    private async Task UpdateDifficultyStatsAsync(string userId, Quiz quiz, double scorePercentage)
    {
        var difficultyStats = await _context.UserDifficultyStats
            .FirstOrDefaultAsync(d => d.UserId == userId && d.Difficulty == quiz.Difficulty);

        if (difficultyStats == null)
        {
            difficultyStats = new UserDifficultyStats
            {
                UserId = userId,
                Difficulty = quiz.Difficulty
            };
            _context.UserDifficultyStats.Add(difficultyStats);
        }

        difficultyStats.TotalQuizzes++;

        difficultyStats.AverageScorePercentage = difficultyStats.AverageScorePercentage == 0
            ? scorePercentage
            : (difficultyStats.AverageScorePercentage + scorePercentage) / 2;

        if (quiz.Score > difficultyStats.HighestScore)
        {
            difficultyStats.HighestScore = quiz.Score;
        }

        if (quiz.StartedAt.HasValue && quiz.CompletedAt.HasValue)
        {
            var completionSeconds = (quiz.CompletedAt.Value - quiz.StartedAt.Value).TotalSeconds;

            if (difficultyStats.FastestCompletionSeconds == 0 || completionSeconds < difficultyStats.FastestCompletionSeconds)
            {
                difficultyStats.FastestCompletionSeconds = completionSeconds;
            }
        }

        if (scorePercentage >= GameConstants.WinThresholdPercentage)
        {
            difficultyStats.TotalWins++;
            difficultyStats.WinStreak++;
        }
        else
        {
            difficultyStats.WinStreak = 0;
        }

        difficultyStats.LastUpdated = DateTime.UtcNow;
    }

    private async Task<IEnumerable<UserStatBoard>> GetRankingsQueryAsync(
        IQueryable<UserStatBoard> query, string statBoardName)
    {
        return statBoardName switch
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

    private static double CalculateSkillScore(UserStatBoard stats)
    {
        // Average score percentage component (40% weight)
        var avgScoreComponent = stats.AverageScorePercentage * 0.4;

        // Win rate component (30% weight)
        var totalChallenges = stats.TotalChallengesWon + stats.TotalChallengesLost;
        var winRate = totalChallenges > 0
            ? (double)stats.TotalChallengesWon / totalChallenges * 100
            : 0;
        var winRateComponent = winRate * 0.3;

        // Difficulty multiplier component (20% weight)
        // More quizzes completed = higher multiplier, capped at 100
        var difficultyComponent = Math.Min(stats.TotalQuizzesCompleted * 2, 100) * 0.2;

        // Consistency component (10% weight)
        // Using longest win streak as a proxy for consistency, capped at 100
        var consistencyComponent = Math.Min(stats.LongestWinStreak * 10, 100) * 0.1;

        return Math.Round(avgScoreComponent + winRateComponent + difficultyComponent + consistencyComponent, 2);
    }
}