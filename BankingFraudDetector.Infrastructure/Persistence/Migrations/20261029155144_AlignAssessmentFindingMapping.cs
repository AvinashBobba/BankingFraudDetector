using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingFraudDetector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignAssessmentFindingMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentFinding_AssessmentRecords_AssessmentRecordId",
                table: "AssessmentFinding");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AssessmentFinding",
                table: "AssessmentFinding");

            migrationBuilder.RenameTable(
                name: "AssessmentFinding",
                newName: "AssessmentFindings");

            migrationBuilder.RenameIndex(
                name: "IX_AssessmentFinding_AssessmentRecordId",
                table: "AssessmentFindings",
                newName: "IX_AssessmentFindings_AssessmentRecordId");

            migrationBuilder.AlterColumn<string>(
                name: "RuleCode",
                table: "AssessmentFindings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Explanation",
                table: "AssessmentFindings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssessmentFindings",
                table: "AssessmentFindings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentFindings_AssessmentRecords_AssessmentRecordId",
                table: "AssessmentFindings",
                column: "AssessmentRecordId",
                principalTable: "AssessmentRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentFindings_AssessmentRecords_AssessmentRecordId",
                table: "AssessmentFindings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AssessmentFindings",
                table: "AssessmentFindings");

            migrationBuilder.RenameTable(
                name: "AssessmentFindings",
                newName: "AssessmentFinding");

            migrationBuilder.RenameIndex(
                name: "IX_AssessmentFindings_AssessmentRecordId",
                table: "AssessmentFinding",
                newName: "IX_AssessmentFinding_AssessmentRecordId");

            migrationBuilder.AlterColumn<string>(
                name: "RuleCode",
                table: "AssessmentFinding",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Explanation",
                table: "AssessmentFinding",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssessmentFinding",
                table: "AssessmentFinding",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentFinding_AssessmentRecords_AssessmentRecordId",
                table: "AssessmentFinding",
                column: "AssessmentRecordId",
                principalTable: "AssessmentRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
