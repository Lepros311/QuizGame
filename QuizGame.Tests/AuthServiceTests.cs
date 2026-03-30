using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuizGame.Core.Entities;
using QuizGame.Core.Interfaces;
using QuizGame.Core.Models.Requests;
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
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Secret", "supersecretkey1234567890abcdefghij" },
                { "Jwt:Issuer", "QuizGame" },
                { "Jwt:Audience", "QuizGameUsers" }
            })
            .Build();

        services.AddSingleton<IConfiguration>(config);
        services.AddScoped<IAuthService, AuthService>();

        services.AddLogging();
        var provider = services.BuildServiceProvider();
        _authService = provider.GetRequiredService<IAuthService>();
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

    [TestMethod]
    public async Task Register_WithDuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser2",
            Email = "duplicate@example.com",
            Password = "Password123!"
        };

        // Act 
        await _authService.RegisterAsync(request);
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.IsTrue(result.Errors.Any());
    }

    [TestMethod]
    public async Task Register_WithWeakPassword_ReturnsFailure()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser3",
            Email = "weakpassword@example.com",
            Password = "abc"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.IsTrue(result.Errors.Any());
    }

    [TestMethod]
    public async Task Register_WithEmptyEmail_ReturnsFailure()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser4",
            Email = "",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.IsTrue(result.Errors.Any());
    }

    [TestMethod]
    public async Task Login_WithValidCredentials_ReturnsJwtToken()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Username = "loginuser",
            Email = "login@example.com",
            Password = "Password123!"
        };

        await _authService.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "login@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.IsTrue(result.Succeeded);
        Assert.IsNotNull(result.Token);
    }

    [TestMethod]
    public async Task Login_WithWrongPassword_ReturnsFailure()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Username = "wrongpassuser",
            Email = "wrongpass@example.com",
            Password = "Password123!"
        };

        await _authService.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "wrongpass@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.IsTrue(result.Errors.Any());
    }

    [TestMethod]
    public async Task Login_WithNonExistentEmail_ReturnsFailure()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nobody@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.IsTrue(result.Errors.Any());
    }
}