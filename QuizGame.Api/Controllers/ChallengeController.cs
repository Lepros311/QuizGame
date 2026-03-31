using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizGame.API.Mappings;
using QuizGame.Core.Interfaces;
using QuizGame.Core.Models.Requests;
using System.Security.Claims;

namespace QuizGame.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChallengeController : ControllerBase
{
    private readonly IChallengeService _challengeService;
    private readonly IQuizConfigurationService _quizConfigurationService;
    private readonly IStatBoardService _statBoardService;

    public ChallengeController(
        IChallengeService challengeService,
        IQuizConfigurationService quizConfigurationService,
        IStatBoardService statBoardService)
    {
        _challengeService = challengeService;
        _quizConfigurationService = quizConfigurationService;
        _statBoardService = statBoardService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateChallengeRequest request)
    {
        try
        {
            var challengerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var config = await _quizConfigurationService.ResolveConfigAsync(
                request.CategoryId,
                request.Difficulty,
                request.QuestionCount,
                request.QuestionTypes,
                true);

            var challenge = await _challengeService.CreateChallengeAsync(
                challengerId,
                config.CategoryId,
                config.Difficulty,
                config.QuestionCount,
                config.QuestionTypes,
                request.OpponentIds);

            return CreatedAtAction(nameof(GetById), new { id = challenge.Id }, challenge.ToDto());
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var challenge = await _challengeService.GetChallengeAsync(id);

        if (challenge == null)
        {
            return NotFound();
        }

        return Ok(challenge.ToDto());
    }

    [HttpGet]
    public async Task<IActionResult> GetUserChallenges()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var challenges = await _challengeService.GetUserChallengesAsync(userId);
        return Ok(challenges.Select(c => c.ToDto()));
    }

    [HttpPatch("{id}/accept")]
    public async Task<IActionResult> Accept(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var challenge = await _challengeService.AcceptChallengeAsync(id, userId);
            return Ok(challenge.ToDto());
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id}/decline")]
    public async Task<IActionResult> Decline(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var challenge = await _challengeService.DeclineChallengeAsync(id, userId);
            return Ok(challenge.ToDto());
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/submit")]
    public async Task<IActionResult> Submit(int id, [FromBody] SubmitAnswersRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var challenge = await _challengeService.SubmitChallengeAnswersAsync(id, userId, request.Answers);
            await _statBoardService.UpdateUserStatsAsync(userId, challenge.QuizId);
            return Ok(challenge.ToDto());
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
