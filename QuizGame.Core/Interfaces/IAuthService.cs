using QuizGame.Core.Models;

namespace QuizGame.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResults> RegisterAsync(RegisterRequest request);
}
