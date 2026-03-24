using QuizGame.Core.Enums;

namespace QuizGame.Core.Entities;

public class ChallengeParticipant
{
    public int Id { get; set; }
    public int ChallengeId { get; set; }
    public Challenge Challenge { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public ParticipantStatus Status { get; set; } = ParticipantStatus.Pending;
    public int? Score { get; set; }
    public DateTime? CompletedAt { get; set; }
}
