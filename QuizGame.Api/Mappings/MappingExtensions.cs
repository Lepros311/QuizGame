using QuizGame.Core.Entities;
using QuizGame.Core.Models.DTOs;

namespace QuizGame.API.Mappings;

public static class MappingExtensions
{
    public static ApplicationUserDto ToDto(this ApplicationUser user, int followersCount = 0, int followingCount = 0)
    {
        return new ApplicationUserDto
        {
            Id = user.Id,
            Username = user.UserName!,
            CreatedAt = user.CreatedAt,
            FollowersCount = followersCount,
            FollowingCount = followingCount
        };
    }

    public static CategoryDto ToDto(this Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }

    public static QuestionDto ToDto(this Question question, bool includeCorrectAnswer = false)
    {
        return new QuestionDto
        {
            Id = question.Id,
            Text = question.Text,
            QuestionType = question.QuestionType,
            Options = question.Options,
            UserAnswer = question.UserAnswer,
            IsCorrect = question.IsCorrect
        };
    }

    public static QuizDto ToDto(this Quiz quiz)
    {
        return new QuizDto
        {
            Id = quiz.Id,
            UserId = quiz.UserId,
            Category = quiz.Category?.ToDto(),
            Difficulty = quiz.Difficulty,
            QuestionCount = quiz.QuestionCount,
            QuestionTypes = quiz.QuestionTypes,
            IsMultiplayer = quiz.IsMultiplayer,
            CreatedAt = quiz.CreatedAt,
            StartedAt = quiz.StartedAt,
            CompletedAt = quiz.CompletedAt,
            Score = quiz.Score,
            Questions = quiz.Questions.Select(q => q.ToDto()).ToList()
        };
    }

    public static ChallengeParticipantDto ToDto(this ChallengeParticipant participant)
    {
        return new ChallengeParticipantDto
        {
            Id = participant.Id,
            UserId = participant.UserId,
            Username = participant.User?.UserName ?? string.Empty,
            Status = participant.Status,
            Score = participant.Score,
            CompletedAt = participant.CompletedAt
        };
    }

    public static ChallengeDto ToDto(this Challenge challenge)
    {
        return new ChallengeDto
        {
            Id = challenge.Id,
            ChallengerId = challenge.ChallengerId,
            ChallengerUsername = challenge.Challenger?.UserName ?? string.Empty,
            Quiz = challenge.Quiz?.ToDto(),
            Status = challenge.Status,
            CreatedAt = challenge.CreatedAt,
            CompletedAt = challenge.CompletedAt,
            Participants = challenge.Participants.Select(p => p.ToDto()).ToList()
        };
    }

    public static NotificationDto ToDto(this Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ChallengeId = notification.ChallengeId
        };
    }

    public static StatBoardDto ToDto(this StatBoard statBoard)
    {
        return new StatBoardDto
        {
            Id = statBoard.Id,
            Name = statBoard.Name,
            Description = statBoard.Description
        };
    }

    public static UserStatBoardDto ToDto(this UserStatBoard stats, string username = "")
    {
        return new UserStatBoardDto
        {
            UserId = stats.UserId,
            Username = username,
            TotalQuizzesCompleted = stats.TotalQuizzesCompleted,
            TotalCorrectAnswers = stats.TotalCorrectAnswers,
            TotalWrongAnswers = stats.TotalWrongAnswers,
            AverageScorePercentage = stats.AverageScorePercentage,
            HighestScore = stats.HighestScore,
            TotalChallengesSent = stats.TotalChallengesSent,
            TotalChallengesReceived = stats.TotalChallengesReceived,
            TotalChallengesWon = stats.TotalChallengesWon,
            TotalChallengesLost = stats.TotalChallengesLost,
            FastestCompletionSeconds = stats.FastestCompletionSeconds,
            AverageCompletionSeconds = stats.AverageCompletionSeconds,
            CurrentWinStreak = stats.CurrentWinStreak,
            LongestWinStreak = stats.LongestWinStreak,
            BestCategory = stats.BestCategory,
            MostPlayedCategory = stats.MostPlayedCategory,
            SkillScore = stats.SkillScore,
            SkillScoreConfidence = stats.SkillScoreConfidence,
            LastUpdated = stats.LastUpdated
        };
    }
}
