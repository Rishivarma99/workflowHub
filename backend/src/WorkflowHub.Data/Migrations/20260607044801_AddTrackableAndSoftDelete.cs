using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkflowHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackableAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workflows_Name",
                table: "workflows");

            migrationBuilder.DropIndex(
                name: "IX_users_Email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_Username",
                table: "users");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "workflows",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "workflows",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "workflows",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "workflows",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "workflow_components",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "workflow_components",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "workflow_components",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "workflow_components",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "workflow_components",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE workflow_components AS c
                SET "CreatedAtUtc" = w."CreatedAtUtc"
                FROM workflows AS w
                WHERE c."WorkflowId" = w."Id";
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "users",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflows_CreatedByUserId",
                table: "workflows",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_Name",
                table: "workflows",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_UpdatedByUserId",
                table: "workflows",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_components_CreatedByUserId",
                table: "workflow_components",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_components_UpdatedByUserId",
                table: "workflow_components",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_CreatedByUserId",
                table: "users",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_users_UpdatedByUserId",
                table: "users",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Username",
                table: "users",
                column: "Username",
                unique: true,
                filter: "\"Username\" IS NOT NULL AND \"IsDeleted\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_users_users_CreatedByUserId",
                table: "users",
                column: "CreatedByUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_users_users_UpdatedByUserId",
                table: "users",
                column: "UpdatedByUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_workflow_components_users_CreatedByUserId",
                table: "workflow_components",
                column: "CreatedByUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_workflow_components_users_UpdatedByUserId",
                table: "workflow_components",
                column: "UpdatedByUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_workflows_users_CreatedByUserId",
                table: "workflows",
                column: "CreatedByUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_workflows_users_UpdatedByUserId",
                table: "workflows",
                column: "UpdatedByUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_users_CreatedByUserId",
                table: "users");

            migrationBuilder.DropForeignKey(
                name: "FK_users_users_UpdatedByUserId",
                table: "users");

            migrationBuilder.DropForeignKey(
                name: "FK_workflow_components_users_CreatedByUserId",
                table: "workflow_components");

            migrationBuilder.DropForeignKey(
                name: "FK_workflow_components_users_UpdatedByUserId",
                table: "workflow_components");

            migrationBuilder.DropForeignKey(
                name: "FK_workflows_users_CreatedByUserId",
                table: "workflows");

            migrationBuilder.DropForeignKey(
                name: "FK_workflows_users_UpdatedByUserId",
                table: "workflows");

            migrationBuilder.DropIndex(
                name: "IX_workflows_CreatedByUserId",
                table: "workflows");

            migrationBuilder.DropIndex(
                name: "IX_workflows_Name",
                table: "workflows");

            migrationBuilder.DropIndex(
                name: "IX_workflows_UpdatedByUserId",
                table: "workflows");

            migrationBuilder.DropIndex(
                name: "IX_workflow_components_CreatedByUserId",
                table: "workflow_components");

            migrationBuilder.DropIndex(
                name: "IX_workflow_components_UpdatedByUserId",
                table: "workflow_components");

            migrationBuilder.DropIndex(
                name: "IX_users_CreatedByUserId",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_Email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_UpdatedByUserId",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_Username",
                table: "users");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "workflows");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "workflows");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "workflows");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "workflow_components");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "workflow_components");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "workflow_components");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "workflow_components");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "workflow_components");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "users");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "users");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "workflows",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflows_Name",
                table: "workflows",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Username",
                table: "users",
                column: "Username",
                unique: true,
                filter: "\"Username\" IS NOT NULL");
        }
    }
}
