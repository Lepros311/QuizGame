using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuizGame.Core.Entities;
using QuizGame.Core.Interfaces;
using QuizGame.Core.Models;
using QuizGame.Infrastructure.Data;
using QuizGame.Infrastructure.Services;

namespace QuizGame.Tests;

[TestClass]
public class AuthServiceTests
{
    private IAuthService _authService = null!;

    [TestInitialize]
    public void Setup()
    {
        var dbContext = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        var userStore = new UserStore<ApplicationUser>(dbContext);
        var userManager = new UserManager<ApplicationUser>(
            userStore,
            null!, // IOptions<IdentityOptions>
            new PasswordHasher<ApplicationUser>(),
            null!, // IUserValidators
            null!, // IPasswordValidators
            null!, // ILookupNormalizer
            null!, // IdentityErrorDescriber
            null!, // IServiceProvider
            null! // ILogger
        );

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Secret", "supersecretkey1234567890abcdefghij" },
                { "Jwt:Issuer", "QuizGame" },
                { "Jwt:Audience", "QuizGameUsers" }
            })
            .Build();

        _authService = new AuthService(userManager, config);
    }

    [TestMethod]
    public async Task Register_WithValidData_ReturnsJwtToken()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.IsTrue(result.Succeeded);
        Assert.IsNotNull(result.Token);
    }
}
