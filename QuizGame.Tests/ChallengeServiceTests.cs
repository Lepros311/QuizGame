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
public class ChallengeServiceTests
{
    private IChallengeService _challengeService = null!;
    private Mock<IQuestionGeneratorService> _mockQuestionGenerator = null!;
    private Mock<INotificationService> _mockNotificationService = null!;
    private Mock<IAnswerGraderService> _mockAnswerGrader = null!;
    private string _challengerId = null!;
    private string _opponentId = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _mockQuestionGenerator = new Mock<IQuestionGeneratorService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockAnswerGrader = new Mock<IAnswerGraderService>();

        _mockQuestionGenerator
            .Setup(x => x.GenerateQuestionsAsync(
                It.IsAny<string>(),
                It.IsAny<Difficulty>(),
                It.IsAny<int>(),
                It.IsAny<List<QuestionType>>()))
            .ReturnsAsync(new List<Question>
            {
                new Question { Text = "Q1", QuestionType = QuestionType.MultipleChoice, Options = ["1", "2", "3", "4"], CorrectAnswer = "4" },
                new Question { Text = "Q2", QuestionType = QuestionType.TrueFalse, Options = [], CorrectAnswer = "false" },
                new Question { Text = "Q3", QuestionType = QuestionType.MultipleChoice, Options = ["1", "2", "3", "4"], CorrectAnswer = "4" },
                new Question { Text = "Q4", QuestionType = QuestionType.TrueFalse, Options = [], CorrectAnswer = "false" },
                new Question { Text = "Q5", QuestionType = QuestionType.MultipleChoice, Options = ["1", "2", "3", "4"], CorrectAnswer = "4" },
                new Question { Text = "Q6", QuestionType = QuestionType.TrueFalse, Options = [], CorrectAnswer = "false" },
                new Question { Text = "Q7", QuestionType = QuestionType.MultipleChoice, Options = ["1", "2", "3", "4"], CorrectAnswer = "4" },
                new Question { Text = "Q8", QuestionType = QuestionType.TrueFalse, Options = [], CorrectAnswer = "false" },
                new Question { Text = "Q9", QuestionType = QuestionType.MultipleChoice, Options = ["1", "2", "3", "4"], CorrectAnswer = "4" },
                new Question { Text = "Q10", QuestionType = QuestionType.TrueFalse, Options = [], CorrectAnswer = "false" },
            });

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddLogging();
        services.AddSingleton(_mockQuestionGenerator.Object);
        services.AddSingleton(_mockNotificationService.Object);
        services.AddSingleton(_mockAnswerGrader.Object);
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IChallengeService, ChallengeService>();

        var provider = services.BuildServiceProvider();

        var dbContext = provider.GetRequiredService<AppDbContext>();
        dbContext.Categories.Add(new Category { Id = 1, Name = "Science", Description = "Science questions" });

        _challengerId = Guid.NewGuid().ToString();
        _opponentId = Guid.NewGuid().ToString();

        dbContext.Users.AddRange(
            new ApplicationUser { Id = _challengerId, UserName = "challenger", Email = "challenger@example.com" },
            new ApplicationUser { Id = _opponentId, UserName = "opponent", Email = "opponent@example.com" }
        );

        await dbContext.SaveChangesAsync();

        _challengeService = provider.GetRequiredService<IChallengeService>();
    }

    [TestMethod]
    public async Task CreateChallenge_WithValidData_ReturnsChallengeWithParticipants()
    {
        // Arrange
        var opponentIds = new List<string> { _opponentId };

        // Act
        var challenge = await _challengeService.CreateChallengeAsync(
            _challengerId,
            1,
            Difficulty.Medium,
            10,
            new List<QuestionType> { QuestionType.MultipleChoice },
            opponentIds);

        // Assert
        Assert.IsNotNull(challenge);
        Assert.IsTrue(challenge.Id > 0);
        Assert.AreEqual(_challengerId, challenge.ChallengerId);
        Assert.AreEqual(1, challenge.Participants.Count);
        Assert.AreEqual(ChallengeStatus.Pending, challenge.Status);
    }

    [TestMethod]
    public async Task CreateChallenge_NotifiesAllOpponents()
    {
        // Arrange
        var opponentIds = new List<string> { _opponentId };

        // Act
        await _challengeService.CreateChallengeAsync(
            _challengerId,
            1,
            Difficulty.Medium,
            10,
            new List<QuestionType> { QuestionType.MultipleChoice },
            opponentIds);

        // Assert
        _mockNotificationService.Verify(x => x.NotifyChallengeReceivedAsync(
            _opponentId,
            It.IsAny<int>(),
            _challengerId), Times.Once);
    }

    [TestMethod]
    public async Task CreateChallenge_WithNoOpponents_ThrowsException()
    {
        // Arrange
        var opponentIds = new List<string>();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            _challengeService.CreateChallengeAsync(
                _challengerId,
                1,
                Difficulty.Medium,
                10,
                new List<QuestionType> { QuestionType.MultipleChoice },
                opponentIds));
    }

    [TestMethod]
    public async Task GetChallenge_WithValidId_ReturnsChallengeWithParticipants()
    {
        // Arrange
        var challenge = await _challengeService.CreateChallengeAsync(
            _challengerId,
            1,
            Difficulty.Medium,
            10,
            new List<QuestionType> { QuestionType.MultipleChoice },
            new List<string> { _opponentId });

        // Act
        var result = await _challengeService.GetChallengeAsync(challenge.Id);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(challenge.Id, result.Id);
        Assert.AreEqual(1, result.Participants.Count);
    }

    [TestMethod]
    public async Task AcceptChallenge_UpdatesParticipantStatus()
    {
        // Arrange
        var challenge = await _challengeService.CreateChallengeAsync(
            _challengerId,
            1,
            Difficulty.Medium,
            10,
            new List<QuestionType> { QuestionType.MultipleChoice },
            new List<string> { _opponentId });

        // Act
        var result = await _challengeService.AcceptChallengeAsync(challenge.Id, _opponentId);

        // Assert
        var participant = result.Participants.First(p => p.UserId == _opponentId);
        Assert.AreEqual(ParticipantStatus.Accepted, participant.Status);
        Assert.AreEqual(ChallengeStatus.Active, result.Status);
    }

    [TestMethod]
    public async Task DeclineChallenge_UpdatesParticipantStatus()
    {
        // Arrange
        var challenge = await _challengeService.CreateChallengeAsync(
            _challengerId,
            1,
            Difficulty.Medium,
            10,
            new List<QuestionType> { QuestionType.MultipleChoice },
            new List<string> { _opponentId });

        // Act
        var result = await _challengeService.DeclineChallengeAsync(challenge.Id, _opponentId);

        // Assert
        var participant = result.Participants.First(p => p.UserId == _opponentId);
        Assert.AreEqual(ParticipantStatus.Declined, participant.Status);
    }

    [TestMethod]
    public async Task AcceptChallenge_WithInvalidChallengeId_ThrowsException()
    {
        // Arrange
        var invalidChallengeId = 999;

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            _challengeService.AcceptChallengeAsync(invalidChallengeId, _opponentId));
    }

    [TestMethod]
    public async Task SubmitChallengeAnswers_UpdatesParticipantScore()
    {
        // Arrange
        var challenge = await _challengeService.CreateChallengeAsync(
            _challengerId,
            1,
            Difficulty.Medium,
            10,
            new List<QuestionType> { QuestionType.MultipleChoice },
            new List<string> { _opponentId });

        await _challengeService.AcceptChallengeAsync(challenge.Id, _opponentId);

        var fullChallenge = await _challengeService.GetChallengeAsync(challenge.Id);
        var answers = new Dictionary<int, string>();
        answers[fullChallenge!.Quiz.Questions[0].Id] = "4";

        // Act
        var result = await _challengeService.SubmitChallengeAnswersAsync(
            challenge.Id,
            _opponentId,
            answers);

        // Assert
        var participant = result.Participants.First(p => p.UserId == _opponentId);
        Assert.AreEqual(1, participant.Score);
        Assert.AreEqual(ParticipantStatus.Completed, participant.Status);
        Assert.IsNotNull(participant.CompletedAt);
    }

    [TestMethod]
    public async Task SubmitChallengeAnswers_WhenAllComplete_ChallengeStatusIsCompleted()
    {
        // Arrange
        var challenge = await _challengeService.CreateChallengeAsync(
            _challengerId,
            1,
            Difficulty.Medium,
            10,
            new List<QuestionType> { QuestionType.MultipleChoice },
            new List<string> { _opponentId });

        await _challengeService.AcceptChallengeAsync(challenge.Id, _opponentId);

        var fullChallenge = await _challengeService.GetChallengeAsync(challenge.Id);
        var answers = new Dictionary<int, string>();
        answers[fullChallenge!.Quiz.Questions[0].Id] = "4";

        // Act
        var result = await _challengeService.SubmitChallengeAnswersAsync(
            challenge.Id,
            _opponentId,
            answers);

        // Assert
        Assert.AreEqual(ChallengeStatus.Completed, result.Status);
        Assert.IsNotNull(result.CompletedAt);
    }

    [TestMethod]
    public async Task GetUserChallenges_ReturnsAllChallengesRegardlessOfRole()
    {
        // Arrange
        await _challengeService.CreateChallengeAsync(
            _challengerId,
            1,
            Difficulty.Medium,
            10,
            new List<QuestionType> { QuestionType.MultipleChoice },
            new List<string> { _opponentId });

        await _challengeService.CreateChallengeAsync(
            _opponentId,
            1,
            Difficulty.Hard,
            10,
            new List<QuestionType> { QuestionType.TrueFalse },
            new List<string> { _challengerId });

        // Act
        var challenges = await _challengeService.GetUserChallengesAsync(_challengerId);

        // Assert
        Assert.AreEqual(2, challenges.Count());
    }

    [TestMethod]
    public async Task AcceptChallenge_WithNonParticipant_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var challenge = await _challengeService.CreateChallengeAsync(
            _challengerId,
            1,
            Difficulty.Medium,
            10,
            new List<QuestionType> { QuestionType.MultipleChoice },
            new List<string> { _opponentId });

        // Act & Assert
        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(() =>
            _challengeService.AcceptChallengeAsync(challenge.Id, Guid.NewGuid().ToString()));
    }

    [TestMethod]
    public async Task DeclineChallenge_WithNonParticipant_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var challenge = await _challengeService.CreateChallengeAsync(
            _challengerId,
            1,
            Difficulty.Medium,
            10,
            new List<QuestionType> { QuestionType.MultipleChoice },
            new List<string> { _opponentId });

        // Act & Assert
        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(() =>
            _challengeService.DeclineChallengeAsync(challenge.Id, Guid.NewGuid().ToString()));
    }

    [TestMethod]
    public async Task SubmitChallengeAnswers_WithNonParticipant_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var challenge = await _challengeService.CreateChallengeAsync(
            _challengerId,
            1,
            Difficulty.Medium,
            10,
            new List<QuestionType> { QuestionType.MultipleChoice },
            new List<string> { _opponentId });

        var answers = new Dictionary<int, string>();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(() =>
            _challengeService.SubmitChallengeAnswersAsync(challenge.Id, Guid.NewGuid().ToString(), answers));
    }
}
