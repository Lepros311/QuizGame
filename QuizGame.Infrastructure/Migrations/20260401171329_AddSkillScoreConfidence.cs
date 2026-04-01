using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillScoreConfidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SkillScoreConfidence",
                table: "UserStatBoards",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkillScoreConfidence",
                table: "UserStatBoards");
        }
    }
}
