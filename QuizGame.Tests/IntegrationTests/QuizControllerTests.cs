using QuizGame.Core.Enums;
using QuizGame.Core.Models.Requests;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace QuizGame.Tests.IntegrationTests;

[TestClass]
[DoNotParallelize]
public class QuizControllerTests : IntegrationTestBase
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
    public async Task CreateQuiz_WithValidData_Returns201()
    {
        // Arrange
        var request = new CreateQuizRequest
        {
            CategoryId = 1,
            Difficulty = Difficulty.Medium,
            QuestionCount = 10,
            QuestionTypes = [QuestionType.MultipleChoice],
            IsMultiplayer = false
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/quiz", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }

    [TestMethod]
    public async Task GetQuiz_WithValidId_Returns200()
    {
        // Arrange
        var createRequest = new CreateQuizRequest
        {
            CategoryId = 1,
            Difficulty = Difficulty.Medium,
            QuestionCount = 10,
            QuestionTypes = [QuestionType.MultipleChoice],
            IsMultiplayer = false
        };

        var createResponse = await Client.PostAsJsonAsync("/api/quiz", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(createContent);
        var quizId = json.RootElement.GetProperty("id").GetInt32();

        // Act
        var response = await Client.GetAsync($"/api/quiz/{quizId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task SubmitAnswers_WithValidData_Returns200()
    {
        // Arrange
        var createRequest = new CreateQuizRequest
        {
            CategoryId = 1,
            Difficulty = Difficulty.Medium,
            QuestionCount = 10,
            QuestionTypes = [QuestionType.MultipleChoice],
            IsMultiplayer = false
        };

        var createResponse = await Client.PostAsJsonAsync("/api/quiz", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(createContent);
        var quizId = json.RootElement.GetProperty("id").GetInt32();

        var questions = json.RootElement.GetProperty("questions");
        var answers = new Dictionary<int, string>();
        foreach (var question in questions.EnumerateArray())
        {
            answers[question.GetProperty("id").GetInt32()] = "A";
        }

        var submitRequest = new SubmitAnswersRequest { Answers = answers };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/quiz/{quizId}/submit", submitRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
