using QuizGame.Core.Enums;
using QuizGame.Core.Models.Requests;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace QuizGame.Tests.IntegrationTests;

[TestClass]
[DoNotParallelize]
public class ChallengeControllerTests : IntegrationTestBase
{
    private string _opponentToken = null!;

    [TestInitialize]
    public async Task Setup()
    {
        InitializeBase();
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);

        _opponentToken = await RegisterAndLoginAsync(
            username: "opponent",
            email: "opponent@example.com",
            password: "Password123!");
    }

    [TestCleanup]
    public void Cleanup()
    {
        CleanupBase();
    }

    private async Task<int> CreateChallengeAsync()
    {
        SetAuthToken(_opponentToken);
        var profileResponse = await Client.GetAsync("/api/user/me");
        var profileContent = await profileResponse.Content.ReadAsStringAsync();
        var profileJson = JsonDocument.Parse(profileContent);
        var opponentId = profileJson.RootElement.GetProperty("userId").GetString()!;

        var challengerToken = await RegisterAndLoginAsync(
            username: "testuser",
            email: "test@example.com",
            password: "Password123!");
        SetAuthToken(challengerToken);

        var request = new CreateChallengeRequest
        {
            CategoryId = 1,
            Difficulty = Difficulty.Medium,
            QuestionCount = 10,
            QuestionTypes = [QuestionType.MultipleChoice],
            OpponentIds = [opponentId]
        };

        var response = await Client.PostAsJsonAsync("/api/challenge", request);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        return json.RootElement.GetProperty("id").GetInt32();
    }

    [TestMethod]
    public async Task CreateChallenge_WithValidData_Returns201()
    {
        // Arrange
        SetAuthToken(_opponentToken);
        var profileResponse = await Client.GetAsync("/api/user/me");
        var profileContent = await profileResponse.Content.ReadAsStringAsync();
        var profileJson = JsonDocument.Parse(profileContent);
        var opponentId = profileJson.RootElement.GetProperty("userId").GetString()!;

        var token = await RegisterAndLoginAsync(
            username: "testuser",
            email: "test@example.com",
            password: "Password123!");
        SetAuthToken(token);

        var request = new CreateChallengeRequest
        {
            CategoryId = 1,
            Difficulty = Difficulty.Medium,
            QuestionCount = 10,
            QuestionTypes = [QuestionType.MultipleChoice],
            OpponentIds = [opponentId]
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/challenge", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }

    [TestMethod]
    public async Task GetChallenge_WithValidId_Returns200()
    {
        // Arrange
        var challengeId = await CreateChallengeAsync();

        // Act
        var response = await Client.GetAsync($"/api/challenge/{challengeId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetUserChallenges_Returns200()
    {
        // Arrange
        await CreateChallengeAsync();

        // Act
        var response = await Client.GetAsync("/api/challenge");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
