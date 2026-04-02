using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using QuizGame.Core.Entities;
using QuizGame.Core.Enums;
using QuizGame.Core.Interfaces;
using QuizGame.Infrastructure.Data;

namespace QuizGame.Tests.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(config =>
        {
            var userSecretsSource = config.Sources.FirstOrDefault(s => s.GetType().Name.Contains("UserSecrets"));
            if (userSecretsSource != null)
            {
                config.Sources.Remove(userSecretsSource);
            }
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Secret", "test-super-secret-key-that-is-long-enough-for-hs256-algorithm" },
                { "Jwt:Issuer", "QuizGame" },
                { "Jwt:Audience", "QuizGameUsers" },
                { "Email:SenderName", "QuizGame" },
                { "Email:SenderEmail", "test@quizgame.com" },
                { "Email:SmtpHost", "localhost" },
                { "Email:SmtpPort", "25" },
                { "Email:SmtpUsername", "test" },
                { "Email:SmtpPassword", "test" },
                { "Gemini:ApiKey", "test-api-key" },
                { "Gemini:Model", "gemini-2.5-flash-lite" },
                { "Admin:Email", "admin@test.com" },
                { "Admin:Password", "Admin123!" },
                { "Admin:Username", "admin" }
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(AppDbContext)).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });

            var questionGeneratorDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IQuestionGeneratorService));
            if (questionGeneratorDescriptor != null)
            {
                services.Remove(questionGeneratorDescriptor);
            }

            var mockQuestionGenerator = new Mock<IQuestionGeneratorService>();
            mockQuestionGenerator
                .Setup(x => x.GenerateQuestionsAsync(
                    It.IsAny<string>(),
                    It.IsAny<Difficulty>(),
                    It.IsAny<int>(),
                    It.IsAny<List<QuestionType>>()))
                .ReturnsAsync(Enumerable.Range(1, 10).Select(i => new Question
                {
                    Text = $"Question {i}",
                    QuestionType = QuestionType.MultipleChoice,
                    Options = ["A", "B", "C", "D"],
                    CorrectAnswer = "A"
                }).ToList());

            services.AddSingleton(mockQuestionGenerator.Object);

            var answerGraderDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IAnswerGraderService));
            if (answerGraderDescriptor != null)
            {
                services.Remove(answerGraderDescriptor);
            }

            var mockAnswerGrader = new Mock<IAnswerGraderService>();
            mockAnswerGrader
                .Setup(x => x.GradeShortAnswerAsync(
                    It.IsAny<Question>(),
                    It.IsAny<string>()))
                .ReturnsAsync(true);

            services.AddSingleton(mockAnswerGrader.Object);

            var emailServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IEmailService));
            if (emailServiceDescriptor != null)
            {
                services.Remove(emailServiceDescriptor);
            }

            var mockEmailService = new Mock<IEmailService>();
            mockEmailService
                .Setup(x => x.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            mockEmailService
                .Setup(x => x.SendChallengeNotificationAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .Returns(Task.CompletedTask);
            mockEmailService
                .Setup(x => x.SendQuizAbandonedReminderAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            services.AddSingleton(mockEmailService.Object);

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
