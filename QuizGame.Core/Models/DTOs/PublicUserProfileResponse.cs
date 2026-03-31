using QuizGame.Core.Entities;

namespace QuizGame.Core.Models;

public class PublicUserProfileResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime MemberSince { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public int SkillScore { get; set; }
    public UserStatBoard? Stats { get; set; }
    public IEnumerable<UserDifficultyStats> DifficultyStats { get; set; } = [];
}
