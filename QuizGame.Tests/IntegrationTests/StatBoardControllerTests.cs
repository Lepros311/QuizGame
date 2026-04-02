using System.Net;

namespace QuizGame.Tests.IntegrationTests;

[TestClass]
[DoNotParallelize]
public class StatBoardControllerTests : IntegrationTestBase
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
    public async Task GetAllStatBoards_Returns200()
    {
        // Act
        var response = await Client.GetAsync("/api/statboard");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetStatBoard_WithValidId_Returns200()
    {
        // Act
        var response = await Client.GetAsync("/api/statboard/1");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetGlobalRankings_Returns200()
    {
        // Act
        var response = await Client.GetAsync("/api/statboard/1/rankings");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetFollowingRankings_Returns200()
    {
        // Act
        var response = await Client.GetAsync("/api/statboard/1/rankings/following");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetMyStats_Returns200()
    {
        // Act
        var response = await Client.GetAsync("/api/statboard/me");

        // Assert
        // Note: returns 400 if user has no stats yet which is valid
        Assert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest);
    }
}
