using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuizGame.Core.Interfaces;
using QuizGame.Infrastructure.Data;
using QuizGame.Infrastructure.Services;

namespace QuizGame.Tests;

[TestClass]
public class CategoryServiceTests
{
    private ICategoryService _categoryService = null!;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddMemoryCache();

        services.AddLogging();
        services.AddScoped<ICategoryService, CategoryService>();

        var provider = services.BuildServiceProvider();
        _categoryService = provider.GetRequiredService<ICategoryService>();
    }

    [TestMethod]
    public async Task GetAllCategories_WhenCategoriesExist_ReturnsAllCategories()
    {
        // Arrange
        await _categoryService.CreateCategoryAsync("Science", "Questions about science");
        await _categoryService.CreateCategoryAsync("History", "Questions about history");

        // Act
        var categories = await _categoryService.GetAllCategoriesAsync();

        // Assert
        Assert.AreEqual(2, categories.Count());
    }

    [TestMethod]
    public async Task GetAllCategories_WhenNoCategoriesExist_ReturnsEmptyList()
    {
        // Arrange - nothing, database is empty

        // Act
        var categories = await _categoryService.GetAllCategoriesAsync();

        // Assert
        Assert.IsNotNull(categories);
        Assert.AreEqual(0, categories.Count());
    }

    [TestMethod]
    public async Task CreateCategory_WithValidData_ReturnsCategoryWithId()
    {
        // Arrange
        var name = "Science";
        var description = "Questions about science";

        // Act
        var category = await _categoryService.CreateCategoryAsync(name, description);

        // Assert
        Assert.IsTrue(category.Id > 0);
        Assert.AreEqual(name, category.Name);
        Assert.AreEqual(description, category.Description);
    }

    [TestMethod]
    public async Task CreateCategory_WithEmptyName_ThrowsException()
    {
        // Arrange
        // Act & Assert - we expect an exception for invalid data
        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            _categoryService.CreateCategoryAsync("", "Some description"));
    }
}
