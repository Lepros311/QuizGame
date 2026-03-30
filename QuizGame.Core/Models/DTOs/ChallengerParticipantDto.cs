using QuizGame.Core.Enums;

namespace QuizGame.Core.Models.DTOs;

public class ChallengeParticipantDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public ParticipantStatus Status { get; set; }
    public int? Score { get; set; }
    public DateTime? CompletedAt { get; set; }
}
