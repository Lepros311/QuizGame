using Microsoft.EntityFrameworkCore;
using QuizGame.Core.Entities;
using QuizGame.Core.Interfaces;
using QuizGame.Core.Models;
using QuizGame.Infrastructure.Data;

namespace QuizGame.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<IEnumerable<ApplicationUser>> SearchUsersAsync(string query)
    {
        return await _context.Users.Where(u => u.UserName!.Contains(query)).ToListAsync();
    }

    public async Task<IEnumerable<ApplicationUser>> GetSuggestedMatchesAsync(string userId)
    {
        var userStats = await _context.UserStatBoards.FirstOrDefaultAsync(s => s.UserId == userId);

        if (userStats == null)
        {
            return Enumerable.Empty<ApplicationUser>();
        }

        var similarUserIds = await _context.UserStatBoards
            .Where(u => u.UserId != userId && u.SkillScore >= userStats.SkillScore - 10 && u.SkillScore <= userStats.SkillScore + 10)
            .Select(u => u.UserId)
            .ToListAsync();

        return await _context.Users.Where(u => similarUserIds.Contains(u.Id)).ToListAsync();
    }

    public async Task<UserProfileResponse> GetUserProfileAsync(string userId)
    {
        var data = await GetUserDataAsync(userId);

        return new UserProfileResponse
        {
            UserId = data.User.Id,
            Username = data.User.UserName!,
            Email = data.User.Email!,
            MemberSince = data.User.CreatedAt,
            FollowersCount = data.FollowersCount,
            FollowingCount = data.FollowingCount,
            SkillScore = (int)Math.Round(data.Stats?.SkillScore ?? 0),
            Stats = data.Stats,
            DifficultyStats = data.DifficultyStats
        };
    }

    public async Task<PublicUserProfileResponse> GetPublicUserProfileAsync(string userId)
    {
        var data = await GetUserDataAsync(userId);

        return new PublicUserProfileResponse
        {
            UserId = data.User.Id,
            Username = data.User.UserName!,
            MemberSince = data.User.CreatedAt,
            FollowersCount = data.FollowersCount,
            FollowingCount = data.FollowingCount,
            SkillScore = (int)Math.Round(data.Stats?.SkillScore ?? 0),
            Stats = data.Stats,
            DifficultyStats = data.DifficultyStats
        };
    }

    private async Task<UserData> GetUserDataAsync(string userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new ArgumentException("User not found.", nameof(userId));
        }

        var stats = await _context.UserStatBoards
            .FirstOrDefaultAsync(u => u.UserId == userId);

        var difficultyStats = await _context.UserDifficultyStats
            .Where(d => d.UserId == userId)
            .ToListAsync();

        var followersCount = await _context.UserFollows
            .CountAsync(f => f.FollowingId == userId);

        var followingCount = await _context.UserFollows
            .CountAsync(f => f.FollowerId == userId);

        return new UserData
        {
            User = user,
            Stats = stats,
            DifficultyStats = difficultyStats,
            FollowersCount = followersCount,
            FollowingCount = followingCount
        };
    }

    private class UserData
    {
        public ApplicationUser User { get; set; } = null!;
        public UserStatBoard? Stats { get; set; }
        public List<UserDifficultyStats> DifficultyStats { get; set; } = [];
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
    }
}
