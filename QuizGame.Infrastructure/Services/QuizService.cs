using Microsoft.EntityFrameworkCore;
using QuizGame.Core;
using QuizGame.Core.Entities;
using QuizGame.Core.Enums;
using QuizGame.Core.Interfaces;
using QuizGame.Infrastructure.Data;

namespace QuizGame.Infrastructure.Services;

public class QuizService : IQuizService
{
    private readonly AppDbContext _context;
    private readonly IQuestionGeneratorService _questionGenerator;
    private readonly IAnswerGraderService _answerGrader;

    public QuizService(AppDbContext context, IQuestionGeneratorService questionGenerator, IAnswerGraderService answerGrader)
    {
        _context = context;
        _questionGenerator = questionGenerator;
        _answerGrader = answerGrader;
    }

    public async Task<Quiz> CreateQuizAsync(string userId, int categoryId, Difficulty difficulty, int questionCount, List<QuestionType> questionTypes, bool isMultiplayer)
    {
        if (questionCount < GameConstants.MinQuestionCount || questionCount > GameConstants.MaxQuestionCount)
        {
            throw new ArgumentException($"Question count must be between {GameConstants.MinQuestionCount} and {GameConstants.MaxQuestionCount}.", nameof(questionCount));
        }

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

    public async Task<Quiz> SubmitAnswersAsync(int quizId, string userId, Dictionary<int, string> answers)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz == null)
        {
            throw new ArgumentException("Quiz not found.", nameof(quizId));
        }

        if (quiz.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only submit answers to your own quizzes.");
        }

        var score = 0;

        foreach (var question in quiz.Questions)
        {
            if (!answers.TryGetValue(question.Id, out var userAnswer))
            {
                continue;
            }

            question.UserAnswer = userAnswer;

            if (question.QuestionType == QuestionType.ShortAnswer)
            {
                question.IsCorrect = await _answerGrader.GradeShortAnswerAsync(question, userAnswer);
            }
            else
            {
                question.IsCorrect = string.Equals(userAnswer, question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
            }

            if (question.IsCorrect == true)
            {
                score++;
            }
        }

        quiz.Score = score;
        quiz.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return quiz;
    }
}
