using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumo.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HouseholdMembers_Users_UserId",
                schema: "lumo",
                table: "HouseholdMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Households_Users_OwnerId",
                schema: "lumo",
                table: "Households");

            migrationBuilder.DropTable(
                name: "ExternalLogin",
                schema: "lumo");

            migrationBuilder.DropTable(
                name: "PasswordResetToken",
                schema: "lumo");

            migrationBuilder.DropTable(
                name: "RefreshTokens",
                schema: "lumo");

            migrationBuilder.DropTable(
                name: "ShoppingItems",
                schema: "lumo");

            migrationBuilder.DropTable(
                name: "ShoppingListUsers",
                schema: "lumo");

            migrationBuilder.DropTable(
                name: "UserActivationTokens",
                schema: "lumo");

            migrationBuilder.DropTable(
                name: "UserShares",
                schema: "lumo");

            migrationBuilder.DropTable(
                name: "ShoppingLists",
                schema: "lumo");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "lumo");

            migrationBuilder.DropIndex(
                name: "IX_Households_OwnerId",
                schema: "lumo",
                table: "Households");

            migrationBuilder.DropIndex(
                name: "IX_HouseholdMembers_UserId",
                schema: "lumo",
                table: "HouseholdMembers");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                schema: "lumo",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "lumo",
                table: "HouseholdMembers");

            migrationBuilder.DropColumn(
                name: "InvitedByUserId",
                schema: "lumo",
                table: "HouseholdInvitations");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "lumo",
                table: "GmailOAuthStates");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "lumo",
                table: "GmailCredentials");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "lumo",
                table: "AgentTasks");

            migrationBuilder.AddColumn<string>(
                name: "OwnerOid",
                schema: "lumo",
                table: "Households",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserOid",
                schema: "lumo",
                table: "HouseholdMembers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InvitedByUserOid",
                schema: "lumo",
                table: "HouseholdInvitations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserOid",
                schema: "lumo",
                table: "GmailOAuthStates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserOid",
                schema: "lumo",
                table: "GmailCredentials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserOid",
                schema: "lumo",
                table: "AgentTasks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerOid",
                schema: "lumo",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "UserOid",
                schema: "lumo",
                table: "HouseholdMembers");

            migrationBuilder.DropColumn(
                name: "InvitedByUserOid",
                schema: "lumo",
                table: "HouseholdInvitations");

            migrationBuilder.DropColumn(
                name: "UserOid",
                schema: "lumo",
                table: "GmailOAuthStates");

            migrationBuilder.DropColumn(
                name: "UserOid",
                schema: "lumo",
                table: "GmailCredentials");

            migrationBuilder.DropColumn(
                name: "UserOid",
                schema: "lumo",
                table: "AgentTasks");

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                schema: "lumo",
                table: "Households",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                schema: "lumo",
                table: "HouseholdMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InvitedByUserId",
                schema: "lumo",
                table: "HouseholdInvitations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                schema: "lumo",
                table: "GmailOAuthStates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                schema: "lumo",
                table: "GmailCredentials",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                schema: "lumo",
                table: "AgentTasks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "lumo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HouseholdId = table.Column<int>(type: "int", nullable: true),
                    Activated = table.Column<bool>(type: "bit", nullable: false),
                    Blocked = table.Column<bool>(type: "bit", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GoogleId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Households_HouseholdId",
                        column: x => x.HouseholdId,
                        principalSchema: "lumo",
                        principalTable: "Households",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ExternalLogin",
                schema: "lumo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProviderUserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLogin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalLogin_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "lumo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetToken",
                schema: "lumo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetToken_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "lumo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                schema: "lumo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AbsoluteExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReplacedByTokenId = table.Column<int>(type: "int", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "lumo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingLists",
                schema: "lumo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HouseholdId = table.Column<int>(type: "int", nullable: true),
                    OwnerId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingLists_Households_HouseholdId",
                        column: x => x.HouseholdId,
                        principalSchema: "lumo",
                        principalTable: "Households",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShoppingLists_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "lumo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserActivationTokens",
                schema: "lumo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivationTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActivationTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "lumo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserShares",
                schema: "lumo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerUserId = table.Column<int>(type: "int", nullable: false),
                    SharedUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserShares_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalSchema: "lumo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserShares_Users_SharedUserId",
                        column: x => x.SharedUserId,
                        principalSchema: "lumo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingItems",
                schema: "lumo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShoppingListId = table.Column<int>(type: "int", nullable: false),
                    IsChecked = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingItems_ShoppingLists_ShoppingListId",
                        column: x => x.ShoppingListId,
                        principalSchema: "lumo",
                        principalTable: "ShoppingLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingListUsers",
                schema: "lumo",
                columns: table => new
                {
                    ShoppingListsId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingListUsers", x => new { x.ShoppingListsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_ShoppingListUsers_ShoppingLists_ShoppingListsId",
                        column: x => x.ShoppingListsId,
                        principalSchema: "lumo",
                        principalTable: "ShoppingLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShoppingListUsers_Users_UsersId",
                        column: x => x.UsersId,
                        principalSchema: "lumo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Households_OwnerId",
                schema: "lumo",
                table: "Households",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdMembers_UserId",
                schema: "lumo",
                table: "HouseholdMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogin_UserId",
                schema: "lumo",
                table: "ExternalLogin",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetToken_UserId",
                schema: "lumo",
                table: "PasswordResetToken",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                schema: "lumo",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingItems_ShoppingListId",
                schema: "lumo",
                table: "ShoppingItems",
                column: "ShoppingListId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingLists_HouseholdId",
                schema: "lumo",
                table: "ShoppingLists",
                column: "HouseholdId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingLists_OwnerId",
                schema: "lumo",
                table: "ShoppingLists",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListUsers_UsersId",
                schema: "lumo",
                table: "ShoppingListUsers",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivationTokens_UserId",
                schema: "lumo",
                table: "UserActivationTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "lumo",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_GoogleId",
                schema: "lumo",
                table: "Users",
                column: "GoogleId",
                unique: true,
                filter: "[GoogleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_HouseholdId",
                schema: "lumo",
                table: "Users",
                column: "HouseholdId");

            migrationBuilder.CreateIndex(
                name: "IX_UserShares_OwnerUserId_SharedUserId",
                schema: "lumo",
                table: "UserShares",
                columns: new[] { "OwnerUserId", "SharedUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserShares_SharedUserId",
                schema: "lumo",
                table: "UserShares",
                column: "SharedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_HouseholdMembers_Users_UserId",
                schema: "lumo",
                table: "HouseholdMembers",
                column: "UserId",
                principalSchema: "lumo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Households_Users_OwnerId",
                schema: "lumo",
                table: "Households",
                column: "OwnerId",
                principalSchema: "lumo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
