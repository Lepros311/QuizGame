using Microsoft.EntityFrameworkCore;
using QuizGame.Core.Entities;
using QuizGame.Core.Interfaces;
using QuizGame.Infrastructure.Data;

namespace QuizGame.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public NotificationService(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task NotifyUserAsync(string userId, string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            IsRead = false,
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task NotifyChallengeReceivedAsync(string userId, int challengeId, string challengerName)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = $"You have been challenged by {challengerName}!",
            IsRead = false,
            ChallengeId = challengeId
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.Email != null)
        {
            await _emailService.SendChallengeNotificationAsync(user.Email, challengerName, challengeId);
        }
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId);

        if (notification == null)
        {
            throw new ArgumentException("Notification not found.", nameof(notificationId));
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync();
    }
}
