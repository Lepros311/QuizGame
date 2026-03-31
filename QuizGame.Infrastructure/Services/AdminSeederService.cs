using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using QuizGame.Core.Entities;
using QuizGame.Core.Interfaces;

namespace QuizGame.Infrastructure.Services;

public class AdminSeederService : IAdminSeederService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AdminSeederService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task SeedAdminAsync()
    {
        var adminEmail = _configuration["Admin:Email"];
        var adminPassword = _configuration["Admin:Password"];
        var adminUsername = _configuration["Admin:Username"];

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            return;
        }

        var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin != null)
        {
            return;
        }

        var adminUser = new ApplicationUser
        {
            UserName = adminUsername,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(adminUser, adminPassword);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
