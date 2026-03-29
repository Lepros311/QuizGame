using QuizGame.Core.Entities;
using QuizGame.Core.Models;

namespace QuizGame.Core.Interfaces;

public interface IUserService
{
    Task<ApplicationUser?> GetUserByIdAsync(string userId);
    Task<IEnumerable<ApplicationUser>> SearchUsersAsync(string query);
    Task<IEnumerable<ApplicationUser>> GetSuggestedMatchesAsync(string userId);
    Task<UserProfileResponse> GetUserProfileAsync(string userId);
}
