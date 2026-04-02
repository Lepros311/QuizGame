using QuizGame.Core.Models.Requests;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace QuizGame.Tests.IntegrationTests;

[TestClass]
[DoNotParallelize]
public class UserControllerTests : IntegrationTestBase
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
    public async Task GetMyProfile_Returns200()
    {
        // Act
        var response = await Client.GetAsync("/api/user/me");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("testuser"));
    }

    [TestMethod]
    public async Task SearchUsers_WithValidQuery_Returns200()
    {
        // Act
        var response = await Client.GetAsync("/api/user/search?q=test");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetSuggestedMatches_Returns200()
    {
        // Act
        var response = await Client.GetAsync("/api/user/suggested-matches");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task FollowUser_Returns204()
    {
        // Arrange
        var secondUserToken = await RegisterAndLoginAsync(
            username: "usertofollow",
            email: "usertofollow@example.com",
            password: "Password123!");

        SetAuthToken(secondUserToken);
        var profileResponse = await Client.GetAsync("/api/user/me");
        var profileContent = await profileResponse.Content.ReadAsStringAsync();
        var profileJson = JsonDocument.Parse(profileContent);
        var secondUserId = profileJson.RootElement.GetProperty("userId").GetString()!;

        var firstUserToken = await RegisterAndLoginAsync(
            username: "testuser",
            email: "test@example.com",
            password: "Password123!");
        SetAuthToken(firstUserToken);

        // Act
        var response = await Client.PostAsJsonAsync($"/api/user/{secondUserId}/follow", new { });

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    [TestMethod]
    public async Task ChangePassword_WithValidData_Returns204()
    {
        // Arrange
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "Password123!",
            NewPassword = "NewPassword123!"
        };

        // Act
        var response = await Client.PatchAsJsonAsync("/api/user/me/password", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }
}
