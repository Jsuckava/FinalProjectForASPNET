using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalAspNetProj.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAnalysisPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SurveyQuestionQuestionId",
                table: "Questions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_SurveyQuestionQuestionId",
                table: "Questions",
                column: "SurveyQuestionQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_SurveyQuestion_SurveyQuestionQuestionId",
                table: "Questions",
                column: "SurveyQuestionQuestionId",
                principalTable: "SurveyQuestion",
                principalColumn: "QuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_SurveyQuestion_SurveyQuestionQuestionId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_SurveyQuestionQuestionId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "SurveyQuestionQuestionId",
                table: "Questions");
        }
    }
}
