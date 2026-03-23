using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using QuizGame.Core.Entities;
using QuizGame.Core.Enums;
using QuizGame.Core.Interfaces;
using QuizGame.Infrastructure.Data;
using QuizGame.Infrastructure.Services;

namespace QuizGame.Tests;

[TestClass]
public class QuizServiceTests
{
    private IQuizService _quizService = null!;
    private Mock<IQuestionGeneratorService> _mockQuestionGenerator = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _mockQuestionGenerator = new Mock<IQuestionGeneratorService>();

        _mockQuestionGenerator
            .Setup(x => x.GenerateQuestionsAsync(
                It.IsAny<string>(),
                It.IsAny<Difficulty>(),
                It.IsAny<int>(),
                It.IsAny<List<QuestionType>>()))
            .ReturnsAsync(new List<Question>
            {
                new Question
                {
                    Text = "What is 2 + 2?",
                    QuestionType = QuestionType.MultipleChoice,
                    Options = ["1", "2", "3", "4"],
                    CorrectAnswer = "4"
                },
                new Question
                {
                    Text = "The Earth is flat.",
                    QuestionType = QuestionType.TrueFalse,
                    Options = [],
                    CorrectAnswer = "false"
                }
            });

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddLogging();
        services.AddSingleton(_mockQuestionGenerator.Object);
        services.AddScoped<IQuizService, QuizService>();

        var provider = services.BuildServiceProvider();

        var dbContext = provider.GetRequiredService<AppDbContext>();
        dbContext.Categories.Add(new Category { Id = 1, Name = "Science", Description = "Science questions" });
        await dbContext.SaveChangesAsync();

        _quizService = provider.GetRequiredService<IQuizService>();
    }

    [TestMethod]
    public async Task CreateQuiz_WithValidData_ReturnsQuizWithQuestions()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var categoryId = 1;
        var difficulty = Difficulty.Medium;
        var questionCount = 2;
        var questionTypes = new List<QuestionType> { QuestionType.MultipleChoice, QuestionType.TrueFalse };

        // Act
        var quiz = await _quizService.CreateQuizAsync(userId, categoryId, difficulty, questionCount, questionTypes, false);

        // Assert
        Assert.IsNotNull(quiz);
        Assert.AreEqual(userId, quiz.UserId);
        Assert.AreEqual(2, quiz.Questions.Count);
        Assert.IsTrue(quiz.Id > 0);
    }

    [TestMethod]
    public async Task CreateQuiz_WithInvalidCategoryId_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var invalidCategoryId = 999;

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            _quizService.CreateQuizAsync(
                userId,
                invalidCategoryId,
                Difficulty.Medium,
                2,
                new List<QuestionType> { QuestionType.MultipleChoice },
                false));
    }

    [TestMethod]
    public async Task GetQuiz_WithValidId_ReturnsQuizWithQuestions()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var createdQuiz = await _quizService.CreateQuizAsync(
            userId,
            1,
            Difficulty.Medium,
            2,
            new List<QuestionType> { QuestionType.MultipleChoice },
            false);

        // Act
        var quiz = await _quizService.GetQuizAsync(createdQuiz.Id);

        // Assert
        Assert.IsNotNull(quiz);
        Assert.AreEqual(createdQuiz.Id, quiz.Id);
        Assert.AreEqual(2, quiz.Questions.Count);
    }

    [TestMethod]
    public async Task GetQuiz_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var invalidQuizId = 999;

        // Act
        var quiz = await _quizService.GetQuizAsync(invalidQuizId);

        // Assert
        Assert.IsNull(quiz);
    }
}
