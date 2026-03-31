using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizGame.API.Mappings;
using QuizGame.Core.Interfaces;
using System.Security.Claims;

namespace QuizGame.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatBoardController : ControllerBase
{
    private readonly IStatBoardService _statBoardService;

    public StatBoardController(IStatBoardService statBoardService)
    {
        _statBoardService = statBoardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var statBoards = await _statBoardService.GetAllStatBoardsAsync();
        return Ok(statBoards.Select(s => s.ToDto()));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var statBoard = await _statBoardService.GetStatBoardAsync(id);

        if (statBoard == null)
        {
            return NotFound();
        }

        return Ok(statBoard.ToDto());
    }

    [HttpGet("{id}/rankings")]
    public async Task<IActionResult> GetGlobalRankings(int id)
    {
        try
        {
            var rankings = await _statBoardService.GetGlobalRankingsAsync(id);
            return Ok(rankings.Select(r => r.ToDto()));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}/rankings/following")]
    public async Task<IActionResult> GetFollowingRankings(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var rankings = await _statBoardService.GetFollowingRankingsAsync(id, userId);
            return Ok(rankings.Select(r => r.ToDto()));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyStats()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var stats = await _statBoardService.GetUserStatsAsync(userId);
            return Ok(stats.ToDto());
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
