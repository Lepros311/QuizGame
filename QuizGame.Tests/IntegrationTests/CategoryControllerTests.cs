using QuizGame.Core.Models.Requests;
using System.Net;
using System.Net.Http.Json;

namespace QuizGame.Tests.IntegrationTests;

[TestClass]
[DoNotParallelize]
public class CategoryControllerTests : IntegrationTestBase
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
    public async Task GetAll_ReturnsSeededCategories()
    {
        // Act
        var response = await Client.GetAsync("/api/category");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("Science"));
        Assert.IsTrue(content.Contains("History"));
    }

    [TestMethod]
    public async Task Create_AsNonAdmin_Returns403()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "New Category",
            Description = "Test description"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/category", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
}