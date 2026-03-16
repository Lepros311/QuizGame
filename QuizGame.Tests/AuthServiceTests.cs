using QuizGame.Core.Interfaces;
using QuizGame.Core.Models;

namespace QuizGame.Tests;

[TestClass]
public class AuthServiceTests
{
    private IAuthService _authService = null!;

    [TestInitialize]
    public void Setup()
    {
        // We'll wire this up next
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
