using Microsoft.Extensions.Configuration;
using QuizGame.Core.Entities;
using QuizGame.Core.Interfaces;
using System.Text;
using System.Text.Json;

namespace QuizGame.Infrastructure.Services;

public class GeminiAnswerGraderService : IAnswerGraderService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiAnswerGraderService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"]!;
        _model = configuration["Gemini:Model"]!;
    }

    public async Task<bool> GradeShortAnswerAsync(Question question, string userAnswer)
    {
        var prompt = BuildPrompt(question, userAnswer);
        var requestBody = BuildRequestBody(prompt);
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

        var response = await _httpClient.PostAsync(url,
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return ParseGradingResponse(responseContent);
    }

    private static string BuildPrompt(Question question, string userAnswer)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are grading a quiz answer. Be flexible with spelling, synonyms, and phrasing.");
        sb.AppendLine();
        sb.AppendLine($"Question: {question.Text}");
        sb.AppendLine($"Correct Answer: {question.CorrectAnswer}");
        sb.AppendLine($"User's Answer: {userAnswer}");
        sb.AppendLine();
        sb.AppendLine("Is the user's answer correct? Reply with ONLY \"true\" or \"false\", nothing else.");
        return sb.ToString();
    }

    private static object BuildRequestBody(string prompt)
    {
        return new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.1,
                maxOutputTokens = 10
            }
        };
    }

    private static bool ParseGradingResponse(string responseContent)
    {
        var jsonDoc = JsonDocument.Parse(responseContent);
        var text = jsonDoc
            .RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString()!
            .Trim()
            .ToLower();

        return text == "true";
    }
}
