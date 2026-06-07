using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkflowHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStarsAndDownloadCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InstallCount",
                table: "workflows",
                newName: "DownloadCount");

            migrationBuilder.AddColumn<int>(
                name: "StarCount",
                table: "workflows",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StarCount",
                table: "workflow_components",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "workflow_component_stars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowComponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_component_stars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_component_stars_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflow_component_stars_workflow_components_WorkflowCompon~",
                        column: x => x.WorkflowComponentId,
                        principalTable: "workflow_components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_stars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_stars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_stars_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflow_stars_workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_component_stars_UserId_WorkflowComponentId",
                table: "workflow_component_stars",
                columns: new[] { "UserId", "WorkflowComponentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_component_stars_WorkflowComponentId",
                table: "workflow_component_stars",
                column: "WorkflowComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_stars_UserId_WorkflowId",
                table: "workflow_stars",
                columns: new[] { "UserId", "WorkflowId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_stars_WorkflowId",
                table: "workflow_stars",
                column: "WorkflowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workflow_component_stars");

            migrationBuilder.DropTable(
                name: "workflow_stars");

            migrationBuilder.DropColumn(
                name: "StarCount",
                table: "workflow_components");

            migrationBuilder.DropColumn(
                name: "StarCount",
                table: "workflows");

            migrationBuilder.RenameColumn(
                name: "DownloadCount",
                table: "workflows",
                newName: "InstallCount");
        }
    }
}
