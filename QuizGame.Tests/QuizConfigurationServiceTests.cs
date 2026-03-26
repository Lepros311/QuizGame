using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuizGame.Core.Entities;
using QuizGame.Core.Enums;
using QuizGame.Core.Interfaces;
using QuizGame.Infrastructure.Data;
using QuizGame.Infrastructure.Services;

namespace QuizGame.Tests;

[TestClass]
public class QuizConfigurationServiceTests
{
    private IQuizConfigurationService _quizConfigurationService = null!;

    [TestInitialize]
    public async Task Setup()
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddLogging();
        services.AddScoped<IQuizConfigurationService, QuizConfigurationService>();

        var provider = services.BuildServiceProvider();

        var dbContext = provider.GetRequiredService<AppDbContext>();
        dbContext.Categories.AddRange(
            new Category { Id = 1, Name = "Science", Description = "Science questions" },
            new Category { Id = 2, Name = "History", Description = "History questions" },
            new Category { Id = 3, Name = "Math", Description = "Math questions" }
        );
        await dbContext.SaveChangesAsync();

        _quizConfigurationService = provider.GetRequiredService<IQuizConfigurationService>();
    }

    [TestMethod]
    public async Task ResolveConfig_WithAllValuesProvided_ReturnsExactConfig()
    {
        // Arrange
        var questionTypes = new List<QuestionType> { QuestionType.MultipleChoice };

        // Act
        var config = await _quizConfigurationService.ResolveConfigAsync(
            categoryId: 1,
            difficulty: Difficulty.Medium,
            questionCount: 10,
            questionTypes: questionTypes,
            isMultiplayer: false);

        // Assert
        Assert.AreEqual(1, config.CategoryId);
        Assert.AreEqual(Difficulty.Medium, config.Difficulty);
        Assert.AreEqual(10, config.QuestionCount);
        Assert.AreEqual(1, config.QuestionTypes.Count);
        Assert.IsFalse(config.WasRandomized);
    }

    [TestMethod]
    public async Task ResolveConfig_WithNullCategory_RandomizesCategory()
    {
        // Arrange - pass null for category
        // Act
        var config = await _quizConfigurationService.ResolveConfigAsync(
            categoryId: null,
            difficulty: Difficulty.Medium,
            questionCount: 10,
            questionTypes: new List<QuestionType> { QuestionType.MultipleChoice },
            isMultiplayer: false);

        // Assert
        Assert.IsTrue(config.CategoryId >= 1 && config.CategoryId <= 3);
        Assert.IsTrue(config.WasRandomized);
    }

    [TestMethod]
    public async Task ResolveConfig_WithAllNulls_RandomizesEverything()
    {
        // Arrange - pass null for everything
        // Act
        var config = await _quizConfigurationService.ResolveConfigAsync(
            categoryId: null,
            difficulty: null,
            questionCount: null,
            questionTypes: null,
            isMultiplayer: false);

        // Assert
        Assert.IsTrue(config.CategoryId >= 1 && config.CategoryId <= 3);
        Assert.IsTrue(Enum.IsDefined(typeof(Difficulty), config.Difficulty));
        Assert.IsTrue(config.QuestionCount >= 10 && config.QuestionCount <= 50);
        Assert.IsTrue(config.QuestionTypes.Count >= 1);
        Assert.IsTrue(config.WasRandomized);
    }

    [TestMethod]
    public async Task ResolveConfig_WithInvalidCategoryId_ThrowsException()
    {
        // Arrange
        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            _quizConfigurationService.ResolveConfigAsync(
                categoryId: 999,
                difficulty: Difficulty.Medium,
                questionCount: 10,
                questionTypes: new List<QuestionType> { QuestionType.MultipleChoice },
                isMultiplayer: false));
    }

    [TestMethod]
    public async Task ResolveConfig_WithNoCategories_ThrowsException()
    {
        // Arrange - create a fresh empty database
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddLogging();
        services.AddScoped<IQuizConfigurationService, QuizConfigurationService>();
        var provider = services.BuildServiceProvider();
        var emptyService = provider.GetRequiredService<IQuizConfigurationService>();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            emptyService.ResolveConfigAsync(
                categoryId: null,
                difficulty: null,
                questionCount: null,
                questionTypes: null,
                isMultiplayer: false));
    }
}
