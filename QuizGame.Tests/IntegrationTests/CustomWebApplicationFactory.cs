using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
