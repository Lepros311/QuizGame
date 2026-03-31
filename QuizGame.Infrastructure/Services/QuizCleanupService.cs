using Microsoft.EntityFrameworkCore;
using QuizGame.Core.Interfaces;
using QuizGame.Infrastructure.Data;

namespace QuizGame.Infrastructure.Services;

public class QuizCleanupService : IQuizCleanupService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public QuizCleanupService(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task SendAbandonedQuizRemindersAsync()
    {
        var now = DateTime.UtcNow;

        var twentyFourHourReminders = await _context.Quizzes
            .Include(q => q.User)
            .Where(q => q.CompletedAt == null &&
                        q.CreatedAt <= now.AddHours(-24) &&
                        q.CreatedAt > now.AddHours(-25))
            .ToListAsync();

        foreach (var quiz in twentyFourHourReminders)
        {
            if (quiz.User?.Email != null)
            {
                await _emailService.SendQuizAbandonedReminderAsync(
                    quiz.User.Email,
                    quiz.User.UserName!,
                    quiz.Id,
                    isFinalWarning: false);
            }
        }

        var sixDayReminders = await _context.Quizzes
            .Include(q => q.User)
            .Where(q => q.CompletedAt == null &&
                        q.CreatedAt <= now.AddDays(-6) &&
                        q.CreatedAt > now.AddDays(-6).AddHours(-1))
            .ToListAsync();

        foreach (var quiz in sixDayReminders)
        {
            if (quiz.User?.Email != null)
            {
                await _emailService.SendQuizAbandonedReminderAsync(
                    quiz.User.Email,
                    quiz.User.UserName!,
                    quiz.Id,
                    isFinalWarning: true);
            }
        }
    }

    public async Task DeleteAbandonedQuizzesAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-7);

        var abandonedQuizzes = await _context.Quizzes
            .Where(q => q.CompletedAt == null && q.CreatedAt <= cutoff)
            .ToListAsync();

        _context.Quizzes.RemoveRange(abandonedQuizzes);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpiredCompletedQuizzesAsync()
    {
        var cutoff = DateTime.UtcNow.AddMonths(-12);

        var expiredQuizzes = await _context.Quizzes
            .Where(q => q.CompletedAt != null && q.CompletedAt <= cutoff)
            .ToListAsync();

        _context.Quizzes.RemoveRange(expiredQuizzes);
        await _context.SaveChangesAsync();
    }
}
