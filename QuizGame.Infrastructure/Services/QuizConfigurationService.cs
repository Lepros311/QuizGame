using Microsoft.EntityFrameworkCore;
using QuizGame.Core.Enums;
using QuizGame.Core.Interfaces;
using QuizGame.Core.Models;
using QuizGame.Infrastructure.Data;

namespace QuizGame.Infrastructure.Services;

public class QuizConfigurationService : IQuizConfigurationService
{
    private readonly AppDbContext _context;
    private readonly Random _random = new();

    public QuizConfigurationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<QuizConfig> ResolveConfigAsync(
        int? categoryId,
        Difficulty? difficulty,
        int? questionCount,
        List<QuestionType>? questionTypes,
        bool isMultiplayer)
    {
        var wasRandomized = false;

        int resolvedCategoryId;
        string resolvedCategoryName;

        if (categoryId == null)
        {
            wasRandomized = true;
            var categories = await _context.Categories.ToListAsync();

            if (!categories.Any())
            {
                throw new InvalidOperationException("No categories available for randomization.");
            }

            var randomCategory = categories[_random.Next(categories.Count)];
            resolvedCategoryId = randomCategory.Id;
            resolvedCategoryName = randomCategory.Name;
        }
        else
        {
            var category = await _context.Categories.FindAsync(categoryId);

            if (category == null)
            {
                throw new ArgumentException("Category not found.", nameof(categoryId));
            }

            resolvedCategoryId = category.Id;
            resolvedCategoryName = category.Name;
        }

        Difficulty resolvedDifficulty;

        if (difficulty == null)
        {
            wasRandomized = true;
            var difficulties = Enum.GetValues<Difficulty>();
            resolvedDifficulty = difficulties[_random.Next(difficulties.Length)];
        }
        else
        {
            resolvedDifficulty = difficulty.Value;
        }

        int resolvedQuestionCount;

        if (questionCount == null)
        {
            wasRandomized = true;
            resolvedQuestionCount = _random.Next(10, 51);
        }
        else
        {
            resolvedQuestionCount = questionCount.Value;
        }

        List<QuestionType> resolvedQuestionTypes;

        if (questionTypes == null || !questionTypes.Any())
        {
            wasRandomized = true;
            var allTypes = Enum.GetValues<QuestionType>().ToList();
            var count = _random.Next(1, allTypes.Count + 1);
            resolvedQuestionTypes = allTypes
                .OrderBy(_ => _random.Next())
                .Take(count)
                .ToList();
        }
        else
        {
            resolvedQuestionTypes = questionTypes;
        }

        return new QuizConfig
        {
            CategoryId = resolvedCategoryId,
            CategoryName = resolvedCategoryName,
            Difficulty = resolvedDifficulty,
            QuestionCount = resolvedQuestionCount,
            QuestionTypes = resolvedQuestionTypes,
            IsMultiplayer = isMultiplayer,
            WasRandomized = wasRandomized
        };
    }
}
