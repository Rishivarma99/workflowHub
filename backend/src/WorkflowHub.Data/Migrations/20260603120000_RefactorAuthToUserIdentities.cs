using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WorkflowHub.Data.Persistence;

#nullable disable

namespace WorkflowHub.Data.Migrations;

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260603120000_RefactorAuthToUserIdentities")]
public partial class RefactorAuthToUserIdentities : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "user_identities",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Provider = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                ProviderSub = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                ProviderEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                ProviderAvatarUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                LastLoginAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user_identities", x => x.Id);
                table.ForeignKey(
                    name: "FK_user_identities_users_UserId",
                    column: x => x.UserId,
                    principalTable: "users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_user_identities_Provider_ProviderSub",
            table: "user_identities",
            columns: new[] { "Provider", "ProviderSub" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_user_identities_UserId",
            table: "user_identities",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_user_identities_UserId_Provider",
            table: "user_identities",
            columns: new[] { "UserId", "Provider" },
            unique: true);

        migrationBuilder.Sql(
            """
            INSERT INTO user_identities ("Id", "UserId", "Provider", "ProviderSub", "ProviderEmail", "ProviderAvatarUrl", "CreatedAtUtc")
            SELECT gen_random_uuid(), "Id", 'google', "GoogleSub", "Email", "GoogleAvatarUrl", "CreatedAtUtc"
            FROM users
            WHERE "GoogleSub" IS NOT NULL AND "GoogleSub" <> '';
            """);

        migrationBuilder.DropIndex(
            name: "IX_users_GoogleSub",
            table: "users");

        migrationBuilder.DropColumn(
            name: "GoogleSub",
            table: "users");

        migrationBuilder.DropColumn(
            name: "GoogleAvatarUrl",
            table: "users");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "GoogleAvatarUrl",
            table: "users",
            type: "character varying(2048)",
            maxLength: 2048,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "GoogleSub",
            table: "users",
            type: "character varying(128)",
            maxLength: 128,
            nullable: false,
            defaultValue: "");

        migrationBuilder.Sql(
            """
            UPDATE users u
            SET "GoogleSub" = i."ProviderSub",
                "GoogleAvatarUrl" = i."ProviderAvatarUrl"
            FROM user_identities i
            WHERE i."UserId" = u."Id" AND i."Provider" = 'google';
            """);

        migrationBuilder.CreateIndex(
            name: "IX_users_GoogleSub",
            table: "users",
            column: "GoogleSub",
            unique: true);

        migrationBuilder.DropTable(
            name: "user_identities");
    }
}
