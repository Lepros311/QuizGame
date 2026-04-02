using Microsoft.AspNetCore.Mvc.Testing;
using QuizGame.Core.Models.Requests;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace QuizGame.Tests.IntegrationTests;

public abstract class IntegrationTestBase
{
    protected HttpClient Client = null!;
    protected CustomWebApplicationFactory Factory = null!;

    protected void InitializeBase()
    {
        Factory = new CustomWebApplicationFactory();
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    protected void CleanupBase()
    {
        Client.Dispose();
        Factory.Dispose();
    }

    protected async Task<string> RegisterAndLoginAsync(
        string username = "testuser",
        string email = "test@example.com",
        string password = "Password123!")
    {
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Username = username,
            Email = email,
            Password = password
        });

        // *** DEBUG START ***
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Register status: {registerResponse.StatusCode}");
        Console.WriteLine($"Register content: {registerContent}");
        // *** DEBUG END ***

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });

        var content = await loginResponse.Content.ReadAsStringAsync();

        // *** DEBUG START ***
        Console.WriteLine($"Login status: {loginResponse.StatusCode}");
        Console.WriteLine($"Login content: {content}");
        // *** DEBUG END ***

        var json = JsonDocument.Parse(content);
        string? token = null;
        if (json.RootElement.TryGetProperty("token", out var tokenElement) ||
            json.RootElement.TryGetProperty("Token", out tokenElement))
        {
            token = tokenElement.GetString();
        }

        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException($"Could not extract token from response: {content}");
        }

        return token;
    }

    protected void SetAuthToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        Console.WriteLine($"Token set: {token[..20]}...");
    }
}
