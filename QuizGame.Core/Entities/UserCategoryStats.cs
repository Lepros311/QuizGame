namespace QuizGame.Core.Entities;

public class UserCategoryStats
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int TotalQuizzes { get; set; }
    public double AverageScorePercentage { get; set; }
    public DateTime LastPlayed { get; set; } = DateTime.UtcNow;
}
