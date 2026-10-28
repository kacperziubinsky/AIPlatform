using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace User.Migrations
{
    /// <inheritdoc />
    public partial class AddCvParsing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CV_Users_UserProfileId",
                table: "CV");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CV",
                table: "CV");

            migrationBuilder.DropIndex(
                name: "IX_CV_UserProfileId",
                table: "CV");

            migrationBuilder.DropColumn(
                name: "UserProfileId",
                table: "CV");

            migrationBuilder.RenameTable(
                name: "CV",
                newName: "CVs");

            migrationBuilder.AddColumn<string>(
                name: "ParseError",
                table: "CVs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParseStatus",
                table: "CVs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ParsedAt",
                table: "CVs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParsedData",
                table: "CVs",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawAiJson",
                table: "CVs",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CVs",
                table: "CVs",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CVs_UserId",
                table: "CVs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CVs_Users_UserId",
                table: "CVs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CVs_Users_UserId",
                table: "CVs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CVs",
                table: "CVs");

            migrationBuilder.DropIndex(
                name: "IX_CVs_UserId",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "ParseError",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "ParseStatus",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "ParsedAt",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "ParsedData",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "RawAiJson",
                table: "CVs");

            migrationBuilder.RenameTable(
                name: "CVs",
                newName: "CV");

            migrationBuilder.AddColumn<Guid>(
                name: "UserProfileId",
                table: "CV",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CV",
                table: "CV",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CV_UserProfileId",
                table: "CV",
                column: "UserProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_CV_Users_UserProfileId",
                table: "CV",
                column: "UserProfileId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
