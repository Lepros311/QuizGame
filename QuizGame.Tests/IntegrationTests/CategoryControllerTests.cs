using QuizGame.Core.Models.Requests;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace QuizGame.Tests.IntegrationTests;

[TestClass]
public class CategoryControllerTests : IntegrationTestBase
{
    private string _token = null!;

    [TestInitialize]
    [DoNotParallelize]
    public async Task Setup()
    {
        InitializeBase();
        _token = await RegisterAndLoginAsync();
        SetAuthToken(_token);
    }

    [TestCleanup]
    public void Cleanup()
    {
        CleanupBase();
    }

    [TestMethod]
    public async Task GetAll_ReturnsSeededCategories()
    {
        Console.WriteLine($"Auth header: {Client.DefaultRequestHeaders.Authorization}");

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/category");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var response = await Client.SendAsync(request);
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
