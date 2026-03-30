using QuizGame.Core.Enums;

namespace QuizGame.Core.Models.DTOs;

public class ChallengeDto
{
    public int Id { get; set; }
    public string ChallengerId { get; set; } = string.Empty;
    public string ChallengerUsername { get; set; } = string.Empty;
    public QuizDto? Quiz { get; set; }
    public ChallengeStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<ChallengeParticipantDto> Participants { get; set; } = [];
}
