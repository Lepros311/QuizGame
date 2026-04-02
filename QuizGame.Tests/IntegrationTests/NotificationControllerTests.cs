using QuizGame.Core.Enums;
using QuizGame.Core.Models.Requests;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace QuizGame.Tests.IntegrationTests;

[TestClass]
[DoNotParallelize]
public class NotificationControllerTests : IntegrationTestBase
{
    [TestInitialize]
    public async Task Setup()
    {
        InitializeBase();
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
    }

    [TestCleanup]
    public void Cleanup()
    {
        CleanupBase();
    }

    [TestMethod]
    public async Task GetNotifications_Returns200()
    {
        // Act
        var response = await Client.GetAsync("/api/notification");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task MarkAsRead_WithValidId_Returns204()
    {
        // Arrange
        var opponentToken = await RegisterAndLoginAsync(
            username: "opponent",
            email: "opponent@example.com",
            password: "Password123!");

        SetAuthToken(opponentToken);
        var profileResponse = await Client.GetAsync("/api/user/me");
        var profileContent = await profileResponse.Content.ReadAsStringAsync();
        var profileJson = JsonDocument.Parse(profileContent);
        var opponentId = profileJson.RootElement.GetProperty("userId").GetString()!;

        var challengerToken = await RegisterAndLoginAsync(
            username: "testuser",
            email: "test@example.com",
            password: "Password123!");
        SetAuthToken(challengerToken);

        var challengeRequest = new CreateChallengeRequest
        {
            CategoryId = 1,
            Difficulty = Difficulty.Medium,
            QuestionCount = 10,
            QuestionTypes = [QuestionType.MultipleChoice],
            OpponentIds = [opponentId]
        };
        await Client.PostAsJsonAsync("/api/challenge", challengeRequest);

        SetAuthToken(opponentToken);
        var notificationsResponse = await Client.GetAsync("/api/notification");
        var notificationsContent = await notificationsResponse.Content.ReadAsStringAsync();
        var notificationsJson = JsonDocument.Parse(notificationsContent);
        var notificationId = notificationsJson.RootElement[0].GetProperty("id").GetInt32();

        // Act - mark notification as read
        var response = await Client.PatchAsync($"/api/notification/{notificationId}/read", null);

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }
}
