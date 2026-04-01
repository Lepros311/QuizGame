using QuizGame.Core.Enums;

namespace QuizGame.Core.Models.DTOs;

public class UserStatBoardDto
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int TotalQuizzesCompleted { get; set; }
    public int TotalCorrectAnswers { get; set; }
    public int TotalWrongAnswers { get; set; }
    public double AverageScorePercentage { get; set; }
    public int HighestScore { get; set; }
    public int TotalChallengesSent { get; set; }
    public int TotalChallengesReceived { get; set; }
    public int TotalChallengesWon { get; set; }
    public int TotalChallengesLost { get; set; }
    public double FastestCompletionSeconds { get; set; }
    public double AverageCompletionSeconds { get; set; }
    public int CurrentWinStreak { get; set; }
    public int LongestWinStreak { get; set; }
    public string BestCategory { get; set; } = string.Empty;
    public string MostPlayedCategory { get; set; } = string.Empty;
    public double SkillScore { get; set; }
    public SkillScoreConfidence SkillScoreConfidence { get; set; } = SkillScoreConfidence.Unrated;
    public DateTime LastUpdated { get; set; }
}
