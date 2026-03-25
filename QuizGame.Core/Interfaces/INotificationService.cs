using QuizGame.Core.Entities;

namespace QuizGame.Core.Interfaces;

public interface INotificationService
{
    Task NotifyUserAsync(string userId, string message);
    Task NotifyChallengeReceivedAsync(string userId, int challengeId, string challengerName);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);
    Task MarkAsReadAsync(int notificationId);
}
