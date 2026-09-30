using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentTaskScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                schema: "lumo",
                table: "AgentTasks");

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                schema: "lumo",
                table: "AgentTasks");

            migrationBuilder.DropColumn(
                name: "ParametersJson",
                schema: "lumo",
                table: "AgentTasks");

            migrationBuilder.AlterColumn<string>(
                name: "UserOid",
                schema: "lumo",
                table: "AgentTasks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "lumo",
                table: "AgentTasks",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CronExpression",
                schema: "lumo",
                table: "AgentTasks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instruction",
                schema: "lumo",
                table: "AgentTasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                schema: "lumo",
                table: "AgentTasks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Instruction",
                schema: "lumo",
                table: "AgentTasks");

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                schema: "lumo",
                table: "AgentTasks");

            migrationBuilder.AlterColumn<string>(
                name: "UserOid",
                schema: "lumo",
                table: "AgentTasks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "lumo",
                table: "AgentTasks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "CronExpression",
                schema: "lumo",
                table: "AgentTasks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "lumo",
                table: "AgentTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                schema: "lumo",
                table: "AgentTasks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ParametersJson",
                schema: "lumo",
                table: "AgentTasks",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
