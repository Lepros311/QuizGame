namespace QuizGame.Core.Interfaces;

public interface INotificationService
{
    Task NotifyUserAsync(string userId, string message);
    Task NotifyChallengeReceivedAsync(string userId, int challengeId, string challengerName);
}
