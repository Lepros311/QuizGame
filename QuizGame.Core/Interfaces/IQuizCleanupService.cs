namespace QuizGame.Core.Interfaces;

public interface IQuizCleanupService
{
    Task SendAbandonedQuizRemindersAsync();
    Task DeleteAbandonedQuizzesAsync();
    Task DeleteExpiredCompletedQuizzesAsync();
}
