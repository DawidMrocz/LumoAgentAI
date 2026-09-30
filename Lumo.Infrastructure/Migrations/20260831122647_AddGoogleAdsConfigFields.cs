using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleAdsConfigFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeveloperToken",
                schema: "lumo",
                table: "GoogleConfigs",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoginCustomerId",
                schema: "lumo",
                table: "GoogleConfigs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeveloperToken",
                schema: "lumo",
                table: "GoogleConfigs");

            migrationBuilder.DropColumn(
                name: "LoginCustomerId",
                schema: "lumo",
                table: "GoogleConfigs");
        }
    }
}
