using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuizGame.Core.Entities;
using QuizGame.Core.Interfaces;
using QuizGame.Infrastructure.Data;

namespace QuizGame.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private const string CategoriesCacheKey = "all_categories";

    public CategoryService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        if (_cache.TryGetValue(CategoriesCacheKey, out IEnumerable<Category>? cachedCategories))
        {
            return cachedCategories;
        }

        var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

        _cache.Set(CategoriesCacheKey, categories);

        return categories;
    }

    public async Task<Category> CreateCategoryAsync(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name cannot be empty.", nameof(name));
        }

        var category = new Category
        {
            Name = name,
            Description = description
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        _cache.Remove(CategoriesCacheKey);

        return category;
    }
}
