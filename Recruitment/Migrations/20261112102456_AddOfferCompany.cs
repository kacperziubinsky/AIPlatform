using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruitment.Migrations
{
    /// <inheritdoc />
    public partial class AddOfferCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OfferCompany",
                table: "Applications",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OfferCompany",
                table: "Applications");
        }
    }
}
