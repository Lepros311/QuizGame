namespace QuizGame.Core.Entities;

public class UserStatBoard
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    // Performance
    public int TotalQuizzesCompleted { get; set; }
    public int TotalCorrectAnswers { get; set; }
    public int TotalWrongAnswers { get; set; }
    public double AverageScorePercentage { get; set; }
    public int HighestScore { get; set; }

    // Challenge
    public int TotalChallengesSent { get; set; }
    public int TotalChallengesReceived { get; set; }
    public int TotalChallengesWon { get; set; }
    public int TotalChallengesLost { get; set; }

    // Speed
    public double FastestCompletionSeconds { get; set; }
    public double AverageCompletionSeconds { get; set; }

    // Streak
    public int CurrentWinStreak { get; set; }
    public int LongestWinStreak { get; set; }

    // Category
    public string BestCategory { get; set; } = string.Empty;
    public string MostPlayedCategory { get; set; } = string.Empty;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
