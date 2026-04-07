using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuizGame.API;
using QuizGame.Core.Entities;
using QuizGame.Core.Interfaces;
using QuizGame.Infrastructure.Data;
using QuizGame.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Database
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
});

// Bind JWT validation from IConfiguration after all sources are merged
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IConfiguration>((options, config) =>
    {
        var jwtSecret = config["Jwt:Secret"]!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:5003",
                "http://localhost:4200",
                "https://my-production-ready-domain.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IChallengeService, ChallengeService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStatBoardService, StatBoardService>();
builder.Services.AddScoped<IUserFollowService, UserFollowService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IQuizConfigurationService, QuizConfigurationService>();
builder.Services.AddScoped<IAdminSeederService, AdminSeederService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Gemini HTTP clients
builder.Services.AddHttpClient<IQuestionGeneratorService, GeminiQuestionGeneratorService>();
builder.Services.AddHttpClient<IAnswerGraderService, GeminiAnswerGraderService>();

// Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// Caching
builder.Services.AddMemoryCache();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "QuizGame API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter your JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Hangfire Dashboard (admin only)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthorizationFilter()]
});

// Schedule recurring jobs
RecurringJob.AddOrUpdate<IQuizCleanupService>(
    "send-abandoned-quiz-reminders",
    service => service.SendAbandonedQuizRemindersAsync(),
    Cron.Hourly);

RecurringJob.AddOrUpdate<IQuizCleanupService>(
    "delete-abandoned-quizzes",
    service => service.DeleteAbandonedQuizzesAsync(),
    Cron.Daily);

RecurringJob.AddOrUpdate<IQuizCleanupService>(
    "delete-expired-completed-quizzes",
    service => service.DeleteExpiredCompletedQuizzesAsync(),
    Cron.Monthly);

// Seed admin user on startup
using (var scope = app.Services.CreateScope())
{
    var adminSeeder = scope.ServiceProvider.GetRequiredService<IAdminSeederService>();
    await adminSeeder.SeedAdminAsync();
}

app.Run();

public partial class Program { }