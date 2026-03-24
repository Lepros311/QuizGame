using QuizGame.Core.Enums;

namespace QuizGame.Core.Entities;

public class Challenge
{
    public int Id { get; set; }
    public string ChallengerId { get; set; } = string.Empty;
    public ApplicationUser Challenger { get; set; } = null!;
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    public ChallengeStatus Status { get; set; } = ChallengeStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<ChallengeParticipant> Participants { get; set; } = [];
}
