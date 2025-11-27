using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalAspNetProj.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentsToSurvey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Survey",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Survey");
        }
    }
}
