using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizGame.API.Mappings;
using QuizGame.Core.Interfaces;
using System.Security.Claims;

namespace QuizGame.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return Ok(notifications.Select(n => n.ToDto()));
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _notificationService.MarkAsReadAsync(id, userId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
