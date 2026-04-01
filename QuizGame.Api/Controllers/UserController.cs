using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizGame.API.Mappings;
using QuizGame.Core.Entities;
using QuizGame.Core.Interfaces;
using QuizGame.Core.Models.Requests;
using System.Security.Claims;

namespace QuizGame.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserFollowService _userFollowService;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserController(IUserService userService, IUserFollowService userFollowService, UserManager<ApplicationUser> userManager)
    {
        _userService = userService;
        _userFollowService = userFollowService;
        _userManager = userManager;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var profile = await _userService.GetUserProfileAsync(userId);
            return Ok(profile);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var profile = await _userService.GetPublicUserProfileAsync(id);
            return Ok(profile);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest("Search query cannot be empty.");
        }

        var users = await _userService.SearchUsersAsync(q);
        return Ok(users.Select(u => u.ToDto()));
    }

    [HttpGet("suggested-matches")]
    public async Task<IActionResult> GetSuggestedMatches()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var matches = await _userService.GetSuggestedMatchesAsync(userId);
        return Ok(matches.Select(u => u.ToDto()));
    }

    [HttpPost("{id}/follow")]
    public async Task<IActionResult> Follow(string id)
    {
        try
        {
            var followerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _userFollowService.FollowUserAsync(followerId, id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("{id}/follow")]
    public async Task<IActionResult> Unfollow(string id)
    {
        try
        {
            var followerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _userFollowService.UnfollowUserAsync(followerId, id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}/followers")]
    public async Task<IActionResult> GetFollowers(string id)
    {
        var followers = await _userFollowService.GetFollowersAsync(id);
        return Ok(followers.Select(u => u.ToDto()));
    }

    [HttpGet("{id}/following")]
    public async Task<IActionResult> GetFollowing(string id)
    {
        var following = await _userFollowService.GetFollowingAsync(id);
        return Ok(following.Select(u => u.ToDto()));
    }

    [HttpPatch("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        return NoContent();
    }

    [HttpPatch("me/email")]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!passwordValid)
        {
            return BadRequest("Invalid password.");
        }

        user.Email = request.NewEmail;
        user.NormalizedEmail = request.NewEmail.ToUpper();

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        return NoContent();
    }
}
