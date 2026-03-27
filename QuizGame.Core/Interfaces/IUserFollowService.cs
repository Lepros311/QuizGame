using QuizGame.Core.Entities;

namespace QuizGame.Core.Interfaces;

public interface IUserFollowService
{
    Task FollowUserAsync(string followerId, string followingId);
    Task UnfollowUserAsync(string followerId, string followingId);
    Task<IEnumerable<ApplicationUser>> GetFollowingAsync(string userId);
    Task<IEnumerable<ApplicationUser>> GetFollowersAsync(string userId);
    Task<bool> IsFollowingAsync(string followerId, string followingId);
    Task<int> GetFollowersCountAsync(string userId);
    Task<int> GetFollowingCountAsync(string userId);
}
