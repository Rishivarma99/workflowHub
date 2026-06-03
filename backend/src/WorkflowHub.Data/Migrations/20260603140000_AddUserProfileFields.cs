using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WorkflowHub.Data.Persistence;

#nullable disable

namespace WorkflowHub.Data.Migrations;

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260603140000_AddUserProfileFields")]
public partial class AddUserProfileFields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Role",
            table: "users",
            type: "character varying(80)",
            maxLength: 80,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Team",
            table: "users",
            type: "character varying(80)",
            maxLength: 80,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Username",
            table: "users",
            type: "character varying(30)",
            maxLength: 30,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_users_Username",
            table: "users",
            column: "Username",
            unique: true,
            filter: "\"Username\" IS NOT NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_users_Username",
            table: "users");

        migrationBuilder.DropColumn(
            name: "Role",
            table: "users");

        migrationBuilder.DropColumn(
            name: "Team",
            table: "users");

        migrationBuilder.DropColumn(
            name: "Username",
            table: "users");
    }
}
