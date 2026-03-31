using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using QuizGame.Core.Entities;
using QuizGame.Core.Interfaces;
using QuizGame.Infrastructure.Data;
using QuizGame.Infrastructure.Services;

namespace QuizGame.Tests;

[TestClass]
public class NotificationServiceTests
{
    private INotificationService _notificationService = null!;
    private Mock<IEmailService> _mockEmailService = null!;
    private string _userId = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _mockEmailService = new Mock<IEmailService>();

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddLogging();
        services.AddSingleton(_mockEmailService.Object);
        services.AddScoped<INotificationService, NotificationService>();

        var provider = services.BuildServiceProvider();

        var dbContext = provider.GetRequiredService<AppDbContext>();
        _userId = Guid.NewGuid().ToString();
        dbContext.Users.Add(new ApplicationUser
        {
            Id = _userId,
            UserName = "testuser",
            Email = "test@example.com"
        });
        await dbContext.SaveChangesAsync();

        _notificationService = provider.GetRequiredService<INotificationService>();
    }

    [TestMethod]
    public async Task NotifyUser_CreatesNotificationInDatabase()
    {
        // Arrange
        var message = "You have a new challenge!";

        // Act
        await _notificationService.NotifyUserAsync(_userId, message);

        // Assert
        var notifications = await _notificationService.GetUserNotificationsAsync(_userId);
        Assert.AreEqual(1, notifications.Count());
        Assert.AreEqual(message, notifications.First().Message);
        Assert.IsFalse(notifications.First().IsRead);
    }

    [TestMethod]
    public async Task NotifyChallengeReceived_CreatesNotificationWithChallengeId()
    {
        // Arrange
        var challengeId = 1;
        var challengerName = "challenger";

        // Act
        await _notificationService.NotifyChallengeReceivedAsync(_userId, challengeId, challengerName);

        // Assert
        var notifications = await _notificationService.GetUserNotificationsAsync(_userId);
        Assert.AreEqual(1, notifications.Count());
        Assert.AreEqual(challengeId, notifications.First().ChallengeId);
        Assert.IsFalse(notifications.First().IsRead);
    }

    [TestMethod]
    public async Task MarkAsRead_UpdatesNotificationIsRead()
    {
        // Arrange
        await _notificationService.NotifyUserAsync(_userId, "Test notification");
        var notifications = await _notificationService.GetUserNotificationsAsync(_userId);
        var notificationId = notifications.First().Id;

        // Act
        await _notificationService.MarkAsReadAsync(notificationId);

        // Assert
        var updatedNotifications = await _notificationService.GetUserNotificationsAsync(_userId);
        Assert.IsTrue(updatedNotifications.First().IsRead);
    }

    [TestMethod]
    public async Task GetUserNotifications_WhenNoNotifications_ReturnsEmptyList()
    {
        // Act
        var notifications = await _notificationService.GetUserNotificationsAsync(_userId);

        // Assert
        Assert.IsNotNull(notifications);
        Assert.AreEqual(0, notifications.Count());
    }

    [TestMethod]
    public async Task MarkAsRead_WithInvalidId_ThrowsException()
    {
        // Arrange
        var invalidNotificationId = 999;

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            _notificationService.MarkAsReadAsync(invalidNotificationId));
    }
}
