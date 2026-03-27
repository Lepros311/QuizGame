using Microsoft.EntityFrameworkCore;
using QuizGame.Core.Entities;
using QuizGame.Core.Interfaces;
using QuizGame.Infrastructure.Data;

namespace QuizGame.Infrastructure.Services;

public class UserFollowService : IUserFollowService
{
    private readonly AppDbContext _context;

    public UserFollowService(AppDbContext context)
    {
        _context = context;
    }

    public async Task FollowUserAsync(string followerId, string followingId)
    {
        if (followerId == followingId)
        {
            throw new ArgumentException("Users cannot follow themselves.", nameof(followingId));
        }

        var alreadyFollowing = await _context.UserFollows
            .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

        if (alreadyFollowing)
        {
            throw new InvalidOperationException("User is already following this user.");
        }

        var follow = new UserFollow
        {
            FollowerId = followerId,
            FollowingId = followingId,
        };

        _context.UserFollows.Add(follow);
        await _context.SaveChangesAsync();
    }

    public async Task UnfollowUserAsync(string followerId, string followingId)
    {
        var follow = await _context.UserFollows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

        if (follow == null)
        {
            throw new ArgumentException("Follow relationship not found.", nameof(followingId));
        }

        _context.UserFollows.Remove(follow);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ApplicationUser>> GetFollowingAsync(string userId)
    {
        return await _context.UserFollows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.Following)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApplicationUser>> GetFollowersAsync(string userId)
    {
        return await _context.UserFollows
            .Where(f => f.FollowingId == userId)
            .Select(f => f.Follower)
            .ToListAsync();
    }

    public async Task<bool> IsFollowingAsync(string followerId, string followingId)
    {
        return await _context.UserFollows
            .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
    }

    public async Task<int> GetFollowersCountAsync(string userId)
    {
        return await _context.UserFollows
            .CountAsync(f => f.FollowingId == userId);
    }

    public async Task<int> GetFollowingCountAsync(string userId)
    {
        return await _context.UserFollows
            .CountAsync(f => f.FollowerId == userId);
    }
}
