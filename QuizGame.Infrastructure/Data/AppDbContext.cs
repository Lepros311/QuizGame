using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using QuizGame.Core;
using QuizGame.Core.Entities;
using QuizGame.Core.Enums;
using System.Text.Json;

namespace QuizGame.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Challenge> Challenges { get; set; }
    public DbSet<ChallengeParticipant> ChallengeParticipants { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<StatBoard> StatBoards { get; set; }
    public DbSet<UserStatBoard> UserStatBoards { get; set; }
    public DbSet<UserFollow> UserFollows { get; set; }
    public DbSet<UserDifficultyStats> UserDifficultyStats { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Quiz>()
            .Property(q => q.QuestionTypes)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<List<QuestionType>>(v, (JsonSerializerOptions)null!)!
            )
            .Metadata.SetValueComparer(new ValueComparer<List<QuestionType>>(
                (list1, list2) => list1!.SequenceEqual(list2!),
                list => list.Aggregate(0, (hashCode, item) => HashCode.Combine(hashCode, item.GetHashCode())),
                list => list.ToList()
            ));

        builder.Entity<Question>()
            .Property(q => q.Options)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!)!
            )
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (list1, list2) => list1!.SequenceEqual(list2!),
                list => list.Aggregate(0, (hashCode, item) => HashCode.Combine(hashCode, item.GetHashCode())),
                list => list.ToList()
            ));

        builder.Entity<UserFollow>()
            .HasOne(f => f.Follower)
            .WithMany()
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserFollow>()
            .HasOne(f => f.Following)
            .WithMany()
            .HasForeignKey(f => f.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserDifficultyStats>()
            .HasIndex(u => new { u.UserId, u.Difficulty })
            .IsUnique();

        builder.Entity<Challenge>()
            .HasOne(c => c.Challenger)
            .WithMany()
            .HasForeignKey(c => c.ChallengerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Challenge>()
            .HasOne(c => c.Quiz)
            .WithMany()
            .HasForeignKey(c => c.QuizId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ChallengeParticipant>()
            .HasOne(cp => cp.User)
            .WithMany()
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Quiz>()
            .HasOne(q => q.User)
            .WithMany()
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Art", Description = "Questions about art" },
            new Category { Id = 2, Name = "Geography", Description = "Questions about geography" },
            new Category { Id = 3, Name = "History", Description = "Questions about history" },
            new Category { Id = 4, Name = "Literature", Description = "Questions about literature" },
            new Category { Id = 5, Name = "Mathematics", Description = "Questions about mathematics" },
            new Category { Id = 6, Name = "Movies & TV", Description = "Questions about movies and TV" },
            new Category { Id = 7, Name = "Music", Description = "Questions about music" },
            new Category { Id = 8, Name = "Science", Description = "Questions about science" },
            new Category { Id = 9, Name = "Sports", Description = "Questions about sports" },
            new Category { Id = 10, Name = "Technology", Description = "Questions about technology" }
        );

        builder.Entity<StatBoard>().HasData(
            new StatBoard { Id = 1, Name = StatBoardConstants.TopScores, Description = "Users with the highest scores" },
            new StatBoard { Id = 2, Name = StatBoardConstants.WinStreaks, Description = "Users with the longest win streaks" },
            new StatBoard { Id = 3, Name = StatBoardConstants.FastestCompletions, Description = "Users with the fastest quiz completions" },
            new StatBoard { Id = 4, Name = StatBoardConstants.MostChallengesWon, Description = "Users with the most challenge wins" },
            new StatBoard { Id = 5, Name = StatBoardConstants.MostQuizzesCompleted, Description = "Users who have completed the most quizzes" }
        );
    }
}
