using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuizGame.Core.Entities;
using QuizGame.Core.Interfaces;
using QuizGame.Infrastructure.Data;
using QuizGame.Infrastructure.Services;

namespace QuizGame.Tests;

[TestClass]
public class UserFollowServiceTests
{
    private IUserFollowService _userFollowService = null!;
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
        services.AddScoped<IUserFollowService, UserFollowService>();

        var provider = services.BuildServiceProvider();

        var dbContext = provider.GetRequiredService<AppDbContext>();
        _userId1 = Guid.NewGuid().ToString();
        _userId2 = Guid.NewGuid().ToString();
        _userId3 = Guid.NewGuid().ToString();

        dbContext.Users.AddRange(
            new ApplicationUser { Id = _userId1, UserName = "user1", Email = "user1@example.com" },
            new ApplicationUser { Id = _userId2, UserName = "user2", Email = "user2@example.com" },
            new ApplicationUser { Id = _userId3, UserName = "user3", Email = "user3@example.com" }
        );
        await dbContext.SaveChangesAsync();

        _userFollowService = provider.GetRequiredService<IUserFollowService>();
    }

    [TestMethod]
    public async Task FollowUser_CreatesFollowRelationship()
    {
        // Act
        await _userFollowService.FollowUserAsync(_userId1, _userId2);

        // Assert
        var isFollowing = await _userFollowService.IsFollowingAsync(_userId1, _userId2);
        Assert.IsTrue(isFollowing);
    }

    [TestMethod]
    public async Task UnfollowUser_RemovesFollowRelationship()
    {
        // Arrange
        await _userFollowService.FollowUserAsync(_userId1, _userId2);

        // Act
        await _userFollowService.UnfollowUserAsync(_userId1, _userId2);

        // Assert
        var isFollowing = await _userFollowService.IsFollowingAsync(_userId1, _userId2);
        Assert.IsFalse(isFollowing);
    }

    [TestMethod]
    public async Task GetFollowing_ReturnsAllUsersFollowed()
    {
        // Arrange
        await _userFollowService.FollowUserAsync(_userId1, _userId2);
        await _userFollowService.FollowUserAsync(_userId1, _userId3);

        // Act
        var following = await _userFollowService.GetFollowingAsync(_userId1);

        // Assert
        Assert.AreEqual(2, following.Count());
    }

    [TestMethod]
    public async Task GetFollowers_ReturnsAllFollowers()
    {
        // Arrange
        await _userFollowService.FollowUserAsync(_userId1, _userId3);
        await _userFollowService.FollowUserAsync(_userId2, _userId3);

        // Act
        var followers = await _userFollowService.GetFollowersAsync(_userId3);

        // Assert
        Assert.AreEqual(2, followers.Count());
    }

    [TestMethod]
    public async Task GetFollowersCount_ReturnsCorrectCount()
    {
        // Arrange
        await _userFollowService.FollowUserAsync(_userId1, _userId3);
        await _userFollowService.FollowUserAsync(_userId2, _userId3);

        // Act
        var count = await _userFollowService.GetFollowersCountAsync(_userId3);

        // Assert
        Assert.AreEqual(2, count);
    }

    [TestMethod]
    public async Task GetFollowingCount_ReturnsCorrectCount()
    {
        // Arrange
        await _userFollowService.FollowUserAsync(_userId1, _userId2);
        await _userFollowService.FollowUserAsync(_userId1, _userId3);

        // Act
        var count = await _userFollowService.GetFollowingCountAsync(_userId1);

        // Assert
        Assert.AreEqual(2, count);
    }

    [TestMethod]
    public async Task FollowUser_FollowingYourself_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            _userFollowService.FollowUserAsync(_userId1, _userId1));
    }

    [TestMethod]
    public async Task FollowUser_AlreadyFollowing_ThrowsException()
    {
        // Arrange
        await _userFollowService.FollowUserAsync(_userId1, _userId2);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            _userFollowService.FollowUserAsync(_userId1, _userId2));
    }
}
