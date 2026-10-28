using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruitment.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CvId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecruiterId = table.Column<Guid>(type: "uuid", nullable: true),
                    OfferTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CoverLetter = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    RecruiterNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AiStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AiScore = table.Column<int>(type: "integer", nullable: true),
                    AiAttempts = table.Column<int>(type: "integer", nullable: false),
                    AiError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AiNextAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Evaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EvaluatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Breakdown = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Evaluations_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_AiStatus_AiNextAttemptAt",
                table: "Applications",
                columns: new[] { "AiStatus", "AiNextAttemptAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_OfferId_ApplicantId",
                table: "Applications",
                columns: new[] { "OfferId", "ApplicantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_ApplicationId",
                table: "Evaluations",
                column: "ApplicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Evaluations");

            migrationBuilder.DropTable(
                name: "Applications");
        }
    }
}
