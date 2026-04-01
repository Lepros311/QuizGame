using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuizGame.Core.Entities;
using QuizGame.Core.Interfaces;
using QuizGame.Infrastructure.Data;
using QuizGame.Infrastructure.Services;

namespace QuizGame.Tests;

[TestClass]
public class UserServiceTests
{
    private IUserService _userService = null!;
    private AppDbContext _dbContext = null!;
    private string _userId1 = null!;
    private string _userId2 = null!;
    private string _userId3 = null!;

    [TestInitialize]
    public async Task Setup()
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddLogging();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserFollowService, UserFollowService>();
        services.AddScoped<IStatBoardService, StatBoardService>();

        var provider = services.BuildServiceProvider();

        _dbContext = provider.GetRequiredService<AppDbContext>();

        _userId1 = Guid.NewGuid().ToString();
        _userId2 = Guid.NewGuid().ToString();
        _userId3 = Guid.NewGuid().ToString();

        _dbContext.Users.AddRange(
            new ApplicationUser { Id = _userId1, UserName = "alice", Email = "alice@example.com", CreatedAt = DateTime.UtcNow },
            new ApplicationUser { Id = _userId2, UserName = "bob", Email = "bob@example.com", CreatedAt = DateTime.UtcNow },
            new ApplicationUser { Id = _userId3, UserName = "charlie", Email = "charlie@example.com", CreatedAt = DateTime.UtcNow }
        );

        _dbContext.UserStatBoards.AddRange(
            new UserStatBoard { UserId = _userId1, HighestScore = 90, TotalQuizzesCompleted = 10 },
            new UserStatBoard { UserId = _userId2, HighestScore = 70, TotalQuizzesCompleted = 5 },
            new UserStatBoard { UserId = _userId3, HighestScore = 80, TotalQuizzesCompleted = 8 }
        );

        await _dbContext.SaveChangesAsync();

        _userService = provider.GetRequiredService<IUserService>();
    }

    [TestMethod]
    public async Task GetUserById_WithValidId_ReturnsUser()
    {
        // Act
        var user = await _userService.GetUserByIdAsync(_userId1);

        // Assert
        Assert.IsNotNull(user);
        Assert.AreEqual("alice", user.UserName);
    }

    [TestMethod]
    public async Task GetUserById_WithInvalidId_ReturnsNull()
    {
        // Act
        var user = await _userService.GetUserByIdAsync("invalid-id");

        // Assert
        Assert.IsNull(user);
    }

    [TestMethod]
    public async Task SearchUsers_WithMatchingQuery_ReturnsMatchingUsers()
    {
        // Act
        var users = await _userService.SearchUsersAsync("ali");

        // Assert
        Assert.AreEqual(1, users.Count());
        Assert.AreEqual("alice", users.First().UserName);
    }

    [TestMethod]
    public async Task SearchUsers_WithNoMatches_ReturnsEmptyList()
    {
        // Act
        var users = await _userService.SearchUsersAsync("xyz");

        // Assert
        Assert.IsNotNull(users);
        Assert.AreEqual(0, users.Count());
    }

    [TestMethod]
    public async Task GetUserProfile_WithValidId_ReturnsProfileWithStats()
    {
        // Act
        var profile = await _userService.GetUserProfileAsync(_userId1);

        // Assert
        Assert.IsNotNull(profile);
        Assert.AreEqual("alice", profile.Username);
        Assert.IsNotNull(profile.Stats);
        Assert.AreEqual(90, profile.Stats.HighestScore);
    }

    [TestMethod]
    public async Task GetUserProfile_WithInvalidId_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            _userService.GetUserProfileAsync("invalid-id"));
    }

    [TestMethod]
    public async Task GetSuggestedMatches_ReturnsUsersWithSimilarSkillScore()
    {
        // Arrange
        var user1Stats = await _dbContext.UserStatBoards.FirstAsync(u => u.UserId == _userId1);
        var user2Stats = await _dbContext.UserStatBoards.FirstAsync(u => u.UserId == _userId2);
        var user3Stats = await _dbContext.UserStatBoards.FirstAsync(u => u.UserId == _userId3);
        user1Stats.SkillScore = 50;
        user2Stats.SkillScore = 55;
        user3Stats.SkillScore = 90;
        await _dbContext.SaveChangesAsync();

        // Act
        var matches = await _userService.GetSuggestedMatchesAsync(_userId1);
        var matchList = matches.ToList();

        // Assert
        Assert.AreEqual(2, matchList.Count);
        Assert.AreEqual("bob", matchList.First().UserName);
    }
}
