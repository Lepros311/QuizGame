namespace QuizGame.Core.Entities;

public class UserFollow
{
    public int Id { get; set; }
    public string FollowerId { get; set; } = string.Empty;
    public ApplicationUser Follower { get; set; } = null!;
    public string FollowingId { get; set; } = string.Empty;
    public ApplicationUser Following { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
