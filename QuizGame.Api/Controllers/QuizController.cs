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
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;
    private readonly IQuizConfigurationService _quizConfigurationService;
    private readonly IStatBoardService _statBoardService;

    public QuizController(
        IQuizService quizService,
        IQuizConfigurationService quizConfigurationService,
        IStatBoardService statBoardService)
    {
        _quizService = quizService;
        _quizConfigurationService = quizConfigurationService;
        _statBoardService = statBoardService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuizRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var config = await _quizConfigurationService.ResolveConfigAsync(
                request.CategoryId,
                request.Difficulty,
                request.QuestionCount,
                request.QuestionTypes,
                request.IsMultiplayer);

            var quiz = await _quizService.CreateQuizAsync(
                userId,
                config.CategoryId,
                config.Difficulty,
                config.QuestionCount,
                config.QuestionTypes,
                config.IsMultiplayer);

            return CreatedAtAction(nameof(GetById), new { id = quiz.Id }, quiz.ToDto());
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var quiz = await _quizService.GetQuizAsync(id);

        if (quiz == null)
        {
            return NotFound();
        }

        return Ok(quiz.ToDto());
    }

    [HttpPost("{id}/submit")]
    public async Task<IActionResult> Submit(int id, [FromBody] SubmitAnswersRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var quiz = await _quizService.SubmitAnswersAsync(id, request.Answers);
            await _statBoardService.UpdateUserStatsAsync(userId, quiz.Id);
            return Ok(quiz.ToDto());
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
