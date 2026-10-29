using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingFraudDetector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessmentHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssessmentRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FromInclusiveUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ToExclusiveUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    RiskBand = table.Column<int>(type: "int", nullable: false),
                    RuleSetVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentFinding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvidenceEventIds = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentFinding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentFinding_AssessmentRecords_AssessmentRecordId",
                        column: x => x.AssessmentRecordId,
                        principalTable: "AssessmentRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentFinding_AssessmentRecordId",
                table: "AssessmentFinding",
                column: "AssessmentRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentRecords_AccountId_AssessedAtUtc",
                table: "AssessmentRecords",
                columns: new[] { "AccountId", "AssessedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentFinding");

            migrationBuilder.DropTable(
                name: "AssessmentRecords");
        }
    }
}
