using Microsoft.Extensions.Configuration;
using QuizGame.Core.Entities;
using QuizGame.Core.Enums;
using QuizGame.Core.Interfaces;
using System.Text;
using System.Text.Json;

namespace QuizGame.Infrastructure.Services;

public class GeminiQuestionGeneratorService : IQuestionGeneratorService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiQuestionGeneratorService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"]!;
        _model = configuration["Gemini:Model"]!;
    }

    public async Task<List<Question>> GenerateQuestionsAsync(
        string categoryName,
        Difficulty difficulty,
        int questionCount,
        List<QuestionType> questionTypes)
    {
        var prompt = BuildPrompt(categoryName, difficulty, questionCount, questionTypes);
        var requestBody = BuildRequestBody(prompt);
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

        var response = await _httpClient.PostAsync(url,
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return ParseQuestionsFromResponse(responseContent);
    }

    private static string BuildPrompt(string categoryName, Difficulty difficulty, int questionCount, List<QuestionType> questionTypes)
    {
        var typeDescriptions = questionTypes.Select(t => t switch
        {
            QuestionType.MultipleChoice => "multiple choice (4 options)",
            QuestionType.TrueFalse => "true/false",
            QuestionType.ShortAnswer => "short answer",
            _ => "multiple choice"
        });

        var sb = new StringBuilder();
        sb.AppendLine($"Generate {questionCount} quiz questions about {categoryName} at {difficulty} difficulty level.");
        sb.AppendLine($"Use these question types: {string.Join(", ", typeDescriptions)}.");
        sb.AppendLine("Mix the question types evenly if multiple types are specified.");
        sb.AppendLine();
        sb.AppendLine("Return ONLY a JSON array with no other text, in this exact format:");
        sb.AppendLine("[");
        sb.AppendLine("  {");
        sb.AppendLine("    \"text\": \"Question text here\",");
        sb.AppendLine("    \"questionType\": \"MultipleChoice\",");
        sb.AppendLine("    \"options\": [\"Option A\", \"Option B\", \"Option C\", \"Option D\"],");
        sb.AppendLine("    \"correctAnswer\": \"Option A\"");
        sb.AppendLine("  },");
        sb.AppendLine("  {");
        sb.AppendLine("    \"text\": \"True or false question here\",");
        sb.AppendLine("    \"questionType\": \"TrueFalse\",");
        sb.AppendLine("    \"options\": [],");
        sb.AppendLine("    \"correctAnswer\": \"true\"");
        sb.AppendLine("  },");
        sb.AppendLine("  {");
        sb.AppendLine("    \"text\": \"Short answer question here\",");
        sb.AppendLine("    \"questionType\": \"ShortAnswer\",");
        sb.AppendLine("    \"options\": [],");
        sb.AppendLine("    \"correctAnswer\": \"Expected answer\"");
        sb.AppendLine("  }");
        sb.AppendLine("]");
        sb.AppendLine();
        sb.AppendLine("For MultipleChoice, always provide exactly 4 options.");
        sb.AppendLine("For TrueFalse, options should be empty and correctAnswer should be \"true\" or \"false\".");
        sb.AppendLine("For ShortAnswer, options should be empty.");
        sb.AppendLine("Do not include any markdown, code blocks, or extra text — just the raw JSON array.");

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
                temperature = 0.7,
                maxOutputTokens = 4096
            }
        };
    }

    /// <summary>
    /// Gemini often wraps JSON in ``` or ```json fences despite the prompt. Strip those and isolate the JSON array.
    /// </summary>
    private static string NormalizeQuestionsJson(string raw)
    {
        var t = raw.Trim();

        // Drop opening fence: ```json, ```JSON, or ```
        if (t.StartsWith("```", StringComparison.Ordinal))
        {
            var firstLineBreak = t.IndexOf('\n');
            if (firstLineBreak >= 0)
                t = t[(firstLineBreak + 1)..].Trim();

            var closing = t.LastIndexOf("```", StringComparison.Ordinal);
            if (closing >= 0)
                t = t[..closing].Trim();
        }

        // If there's still prose before the array, take from first '[' to last ']'
        if (!t.StartsWith('['))
        {
            var start = t.IndexOf('[');
            var end = t.LastIndexOf(']');
            if (start >= 0 && end > start)
                t = t[start..(end + 1)];
        }

        return t.Trim();
    }

    private static List<Question> ParseQuestionsFromResponse(string responseContent)
    {
        var jsonDoc = JsonDocument.Parse(responseContent);
        var text = jsonDoc
            .RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString()!;

        var payload = NormalizeQuestionsJson(text);

        var questions = JsonSerializer.Deserialize<List<GeminiQuestionDto>>(payload, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        return questions.Select(q => new Question
        {
            Text = q.Text,
            QuestionType = Enum.Parse<QuestionType>(q.QuestionType),
            Options = q.Options,
            CorrectAnswer = q.CorrectAnswer
        }).ToList();
    }
}

// Internal DTO for deserializing Gemini's response
internal class GeminiQuestionDto
{
    public string Text { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public string CorrectAnswer { get; set; } = string.Empty;
}
