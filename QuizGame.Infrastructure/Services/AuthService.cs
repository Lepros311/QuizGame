using QuizGame.Core.Interfaces;
using QuizGame.Core.Models;

namespace QuizGame.Infrastructure.Services;

public class AuthService : IAuthService
{
    public Task<AuthResults> RegisterAsync(RegisterRequest request)
    {
        throw new NotImplementedException();
    }
}
