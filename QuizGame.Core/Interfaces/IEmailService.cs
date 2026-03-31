namespace QuizGame.Core.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task SendChallengeNotificationAsync(string toEmail, string challengerUsername, int challengeId);
    Task SendQuizAbandonedReminderAsync(string toEmail, string username, int quizId, bool isFinalWarning);
}
