using QuizGame.Core.Enums;

namespace QuizGame.Core.Entities;

public class UserDifficultyStats
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public Difficulty Difficulty { get; set; }
    public int TotalQuizzes { get; set; }
    public double AverageScorePercentage { get; set; }
    public int HighestScore { get; set; }
    public double FastestCompletionSeconds { get; set; }
    public int WinStreak { get; set; }
    public int TotalWins { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
