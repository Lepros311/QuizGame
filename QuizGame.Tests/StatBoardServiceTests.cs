using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuizGame.Core;
using QuizGame.Core.Entities;
using QuizGame.Core.Interfaces;
using QuizGame.Infrastructure.Data;
using QuizGame.Infrastructure.Services;

namespace QuizGame.Tests;

[TestClass]
public class StatBoardServiceTests
{
    private IStatBoardService _statBoardService = null!;
    private AppDbContext _dbContext = null!;
    private string _userId1 = null!;
    private string _userId2 = null!;
    private string _userId3 = null!;

    [TestInitialize]
    public async Task Setup()
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddMemoryCache();

        services.AddLogging();
        services.AddScoped<IStatBoardService, StatBoardService>();

        var provider = services.BuildServiceProvider();

        _dbContext = provider.GetRequiredService<AppDbContext>();

        // Seed users
        _userId1 = Guid.NewGuid().ToString();
        _userId2 = Guid.NewGuid().ToString();
        _userId3 = Guid.NewGuid().ToString();

        _dbContext.Users.AddRange(
            new ApplicationUser { Id = _userId1, UserName = "user1", Email = "user1@example.com" },
            new ApplicationUser { Id = _userId2, UserName = "user2", Email = "user2@example.com" },
            new ApplicationUser { Id = _userId3, UserName = "user3", Email = "user3@example.com" }
        );

        // Seed StatBoards
        _dbContext.StatBoards.AddRange(
            new StatBoard { Id = 1, Name = StatBoardConstants.TopScores, Description = "Highest scores" },
            new StatBoard { Id = 2, Name = StatBoardConstants.WinStreaks, Description = "Longest win streaks" },
            new StatBoard { Id = 3, Name = StatBoardConstants.FastestCompletions, Description = "Fastest quiz completions" },
            new StatBoard { Id = 4, Name = StatBoardConstants.MostChallengesWon, Description = "Most challenges won" },
            new StatBoard { Id = 5, Name = StatBoardConstants.MostQuizzesCompleted, Description = "Most quizzes completed" }
        );

        // Seed UserStatBoards
        _dbContext.UserStatBoards.AddRange(
            new UserStatBoard
            {
                UserId = _userId1,
                HighestScore = 90,
                LongestWinStreak = 5,
                CurrentWinStreak = 3,
                TotalQuizzesCompleted = 10,
                TotalChallengesWon = 7,
                FastestCompletionSeconds = 120
            },
            new UserStatBoard
            {
                UserId = _userId2,
                HighestScore = 75,
                LongestWinStreak = 3,
                CurrentWinStreak = 1,
                TotalQuizzesCompleted = 8,
                TotalChallengesWon = 4,
                FastestCompletionSeconds = 95
            },
            new UserStatBoard
            {
                UserId = _userId3,
                HighestScore = 85,
                LongestWinStreak = 7,
                CurrentWinStreak = 7,
                TotalQuizzesCompleted = 15,
                TotalChallengesWon = 10,
                FastestCompletionSeconds = 180
            }
        );

        await _dbContext.SaveChangesAsync();

        _statBoardService = provider.GetRequiredService<IStatBoardService>();
    }

    [TestMethod]
    public async Task GetAllStatBoards_ReturnsAllStatBoards()
    {
        // Act
        var statBoards = await _statBoardService.GetAllStatBoardsAsync();

        // Assert
        Assert.AreEqual(5, statBoards.Count());
    }

    [TestMethod]
    public async Task GetStatBoard_WithValidId_ReturnsStatBoard()
    {
        // Act
        var statBoard = await _statBoardService.GetStatBoardAsync(1);

        // Assert
        Assert.IsNotNull(statBoard);
        Assert.AreEqual(StatBoardConstants.TopScores, statBoard.Name);
    }

    [TestMethod]
    public async Task GetStatBoard_WithInvalidId_ReturnsNull()
    {
        // Act
        var statBoard = await _statBoardService.GetStatBoardAsync(999);

        // Assert
        Assert.IsNull(statBoard);
    }

    [TestMethod]
    public async Task GetUserStats_WithValidUserId_ReturnsUserStats()
    {
        // Act
        var stats = await _statBoardService.GetUserStatsAsync(_userId1);

        // Assert
        Assert.IsNotNull(stats);
        Assert.AreEqual(90, stats.HighestScore);
        Assert.AreEqual(10, stats.TotalQuizzesCompleted);
    }

    [TestMethod]
    public async Task GetGlobalRankings_TopScores_ReturnsUsersOrderedByHighestScore()
    {
        // Act
        var rankings = await _statBoardService.GetGlobalRankingsAsync(1);

        // Assert - user1 (90) should be first, user3 (85) second, user2 (75) third
        var rankingsList = rankings.ToList();
        Assert.AreEqual(_userId1, rankingsList[0].UserId);
        Assert.AreEqual(_userId3, rankingsList[1].UserId);
        Assert.AreEqual(_userId2, rankingsList[2].UserId);
    }

    [TestMethod]
    public async Task GetFollowingRankings_ReturnsOnlyFollowedUsers()
    {
        // Arrange - user1 follows user2 but not user3
        _dbContext.UserFollows.Add(new UserFollow
        {
            FollowerId = _userId1,
            FollowingId = _userId2
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var rankings = await _statBoardService.GetFollowingRankingsAsync(1, _userId1);

        // Assert - should only see user2 in rankings
        Assert.AreEqual(1, rankings.Count());
        Assert.AreEqual(_userId2, rankings.First().UserId);
    }

    [TestMethod]
    public async Task UpdateUserStats_AfterQuizCompleted_UpdatesStats()
    {
        // Arrange - create a completed quiz for user1
        var category = new Category { Id = 1, Name = "Science", Description = "Science questions" };
        _dbContext.Categories.Add(category);

        var quiz = new Quiz
        {
            UserId = _userId1,
            CategoryId = 1,
            Difficulty = Core.Enums.Difficulty.Medium,
            QuestionCount = 10,
            QuestionTypes = [Core.Enums.QuestionType.MultipleChoice],
            IsMultiplayer = false,
            Score = 8,
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            CompletedAt = DateTime.UtcNow,
            Questions = new List<Question>
            {
                new Question { Text = "Q1", QuestionType = Core.Enums.QuestionType.MultipleChoice, CorrectAnswer = "A", UserAnswer = "A", IsCorrect = true, Options = [] },
                new Question { Text = "Q2", QuestionType = Core.Enums.QuestionType.MultipleChoice, CorrectAnswer = "B", UserAnswer = "B", IsCorrect = true, Options = [] },
                new Question { Text = "Q3", QuestionType = Core.Enums.QuestionType.MultipleChoice, CorrectAnswer = "C", UserAnswer = "D", IsCorrect = false, Options = [] },
            }
        };

        _dbContext.Quizzes.Add(quiz);
        await _dbContext.SaveChangesAsync();

        // Act
        await _statBoardService.UpdateUserStatsAsync(_userId1, quiz.Id);

        // Assert
        var stats = await _statBoardService.GetUserStatsAsync(_userId1);
        Assert.AreEqual(11, stats.TotalQuizzesCompleted); // was 10, now 11
        Assert.IsTrue(stats.TotalCorrectAnswers > 0);
    }

    [TestMethod]
    public async Task UpdateUserStats_AfterQuizCompleted_UpdatesSkillScore()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Science", Description = "Science questions" };
        _dbContext.Categories.Add(category);

        var quiz = new Quiz
        {
            UserId = _userId1,
            CategoryId = 1,
            Difficulty = Core.Enums.Difficulty.Medium,
            QuestionCount = 10,
            QuestionTypes = [Core.Enums.QuestionType.MultipleChoice],
            IsMultiplayer = false,
            Score = 9,
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            CompletedAt = DateTime.UtcNow,
            Questions = new List<Question>
        {
            new Question { Text = "Q1", QuestionType = Core.Enums.QuestionType.MultipleChoice, CorrectAnswer = "A", UserAnswer = "A", IsCorrect = true, Options = [] },
            new Question { Text = "Q2", QuestionType = Core.Enums.QuestionType.MultipleChoice, CorrectAnswer = "B", UserAnswer = "B", IsCorrect = true, Options = [] },
            new Question { Text = "Q3", QuestionType = Core.Enums.QuestionType.MultipleChoice, CorrectAnswer = "C", UserAnswer = "D", IsCorrect = false, Options = [] },
        }
        };

        _dbContext.Quizzes.Add(quiz);
        await _dbContext.SaveChangesAsync();

        // Act
        await _statBoardService.UpdateUserStatsAsync(_userId1, quiz.Id);

        // Assert
        var stats = await _statBoardService.GetUserStatsAsync(_userId1);
        Assert.IsTrue(stats.SkillScore > 0);
    }
}