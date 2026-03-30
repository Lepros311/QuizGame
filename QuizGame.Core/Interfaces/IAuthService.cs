using QuizGame.Core.Models;
using QuizGame.Core.Models.Requests;

namespace QuizGame.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> LoginAsync(LoginRequest request);
}
