using Microsoft.EntityFrameworkCore;
using QuizGame.Core.Entities;
using QuizGame.Core.Enums;
using QuizGame.Core.Interfaces;
using QuizGame.Infrastructure.Data;

namespace QuizGame.Infrastructure.Services;

public class QuizService : IQuizService
{
    private readonly AppDbContext _context;
    private readonly IQuestionGeneratorService _questionGenerator;

    public QuizService(AppDbContext context, IQuestionGeneratorService questionGenerator)
    {
        _context = context;
        _questionGenerator = questionGenerator;
    }

    public async Task<Quiz> CreateQuizAsync(string userId, int categoryId, Difficulty difficulty, int questionCount, List<QuestionType> questionTypes, bool isMultiplayer)
    {
        var category = await _context.Categories.FindAsync(categoryId);

        if (category == null)
        {
            throw new ArgumentException("Category not found.", nameof(categoryId));
        }

        var questions = await _questionGenerator.GenerateQuestionsAsync(
            category.Name,
            difficulty,
            questionCount,
            questionTypes);

        var quiz = new Quiz
        {
            UserId = userId,
            CategoryId = categoryId,
            Difficulty = difficulty,
            QuestionCount = questionCount,
            QuestionTypes = questionTypes,
            IsMultiplayer = isMultiplayer,
            Questions = questions
        };

        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        return quiz;
    }

    public async Task<Quiz?> GetQuizAsync(int quizId)
    {
        return await _context.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == quizId);
    }

    public async Task<Quiz> SubmitAnswersAsync(int quizId, Dictionary<int, string> answers)
    {
        throw new NotImplementedException();
    }
}
