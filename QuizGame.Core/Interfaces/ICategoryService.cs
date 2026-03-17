using QuizGame.Core.Entities;

namespace QuizGame.Core.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Category> CreateCategoryAsync(string name, string description);
}
