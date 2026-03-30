namespace QuizGame.Core.Models.DTOs;

public class ApplicationUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public double SkillScore { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
}
