using Microsoft.EntityFrameworkCore;
using QuizGame.Core.Entities;
using QuizGame.Core.Enums;
using QuizGame.Core.Interfaces;
using QuizGame.Infrastructure.Data;

namespace QuizGame.Infrastructure.Services;

public class ChallengeService : IChallengeService
{
    private readonly AppDbContext _context;
    private readonly IQuizService _quizService;
    private readonly INotificationService _notificationService;

    public ChallengeService(AppDbContext context, IQuizService quizService, INotificationService notificationService)
    {
        _context = context;
        _quizService = quizService;
        _notificationService = notificationService;
    }

    public async Task<Challenge> CreateChallengeAsync(string challengerId, int categoryId, Difficulty difficulty, int questionCount, List<QuestionType> questionTypes, List<string> opponentIds)
    {
        if (opponentIds == null || opponentIds.Count == 0)
        {
            throw new ArgumentException("At least one opponent is required.", nameof(opponentIds));
        }

        var quiz = await _quizService.CreateQuizAsync(
            challengerId,
            categoryId,
            difficulty,
            questionCount,
            questionTypes,
            true);

        var challenge = new Challenge
        {
            ChallengerId = challengerId,
            QuizId = quiz.Id,
            Status = ChallengeStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Participants = opponentIds.Select(opponentId => new ChallengeParticipant
            {
                UserId = opponentId,
                Status = ParticipantStatus.Pending
            }).ToList()
        };

        _context.Challenges.Add(challenge);
        await _context.SaveChangesAsync();

        foreach (var opponentId in opponentIds)
        {
            await _notificationService.NotifyChallengeReceivedAsync(opponentId, challenge.Id, challengerId);
        }

        return challenge;
    }

    public async Task<Challenge?> GetChallengeAsync(int challengeId)
    {
        return await _context.Challenges
            .Include(c => c.Participants)
            .Include(c => c.Quiz)
                .ThenInclude(q => q.Questions)
            .FirstOrDefaultAsync(c => c.Id == challengeId);
    }

    public async Task<IEnumerable<Challenge>> GetUserChallengesAsync(string userId)
    {
        return await _context.Challenges
            .Include(c => c.Participants)
            .Include(c => c.Quiz)
            .Where(c => c.ChallengerId == userId || c.Participants.Any(p => p.UserId == userId))
            .ToListAsync();
    }

    public async Task<Challenge> AcceptChallengeAsync(int challengeId, string userId)
    {
        var challenge = await _context.Challenges
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == challengeId);

        if (challenge == null)
        {
            throw new ArgumentException("Challenge not found.", nameof(challengeId));
        }

        var participant = challenge.Participants.FirstOrDefault(p => p.UserId == userId);

        participant.Status = ParticipantStatus.Accepted;
        challenge.Status = ChallengeStatus.Active;

        await _context.SaveChangesAsync();

        return challenge;
    }

    public async Task<Challenge> DeclineChallengeAsync(int challengeId, string userId)
    {
        var challenge = await _context.Challenges
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == challengeId);

        if (challenge == null)
        {
            throw new ArgumentException("Challenge not found.", nameof(challengeId));
        }

        var participant = challenge.Participants.FirstOrDefault(p => p.UserId == userId);

        if (participant == null)
        {
            throw new ArgumentException("User is not a participant in this challenge.", nameof(userId));
        }

        participant.Status = ParticipantStatus.Declined;

        await _context.SaveChangesAsync();

        return challenge;
    }

    public async Task<Challenge> SubmitChallengeAnswersAsync(int challengeId, string userId, Dictionary<int, string> answers)
    {
        var challenge = await _context.Challenges
            .Include(c => c.Participants)
            .Include(c => c.Quiz)
                .ThenInclude(q => q.Questions)
            .FirstOrDefaultAsync(c => c.Id == challengeId);

        if (challenge == null)
        {
            throw new ArgumentException("Challenge not found.", nameof(challengeId));
        }

        var participant = challenge.Participants.FirstOrDefault(p => p.UserId == userId);

        if (participant == null)
        {
            throw new ArgumentException("User is not a participant in this challenge.", nameof(userId));
        }

        var score = 0;
        foreach (var question in challenge.Quiz.Questions)
        {
            if (!answers.TryGetValue(question.Id, out var userAnswer))
            {
                continue;
            }

            question.UserAnswer = userAnswer;
            question.IsCorrect = string.Equals(userAnswer, question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);

            if (question.IsCorrect == true)
            {
                score++;
            }
        }

        participant.Score = score;
        participant.Status = ParticipantStatus.Completed;
        participant.CompletedAt = DateTime.UtcNow;

        if (challenge.Participants.All(p => p.Status == ParticipantStatus.Completed))
        {
            challenge.Status = ChallengeStatus.Completed;
            challenge.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return challenge;
    }
}
