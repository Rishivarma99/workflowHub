using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkflowHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowPublishTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Tags = table.Column<string[]>(type: "text[]", nullable: false),
                    RepositoryUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CommitSha = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceIde = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Complexity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TargetAudience = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    InstallCount = table.Column<int>(type: "integer", nullable: false),
                    ComponentTypes = table.Column<string[]>(type: "text[]", nullable: false),
                    Dependencies = table.Column<string>(type: "jsonb", nullable: false),
                    SearchText = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflows_users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workflow_components",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    GitHubUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ComponentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Capabilities = table.Column<string>(type: "jsonb", nullable: false),
                    Keywords = table.Column<string[]>(type: "text[]", nullable: false),
                    SearchPhrases = table.Column<string[]>(type: "text[]", nullable: false),
                    Technologies = table.Column<string[]>(type: "text[]", nullable: false),
                    Dependencies = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_components", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_components_workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_components_ComponentType",
                table: "workflow_components",
                column: "ComponentType");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_components_WorkflowId",
                table: "workflow_components",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_Name",
                table: "workflows",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflows_OwnerId",
                table: "workflows",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workflow_components");

            migrationBuilder.DropTable(
                name: "workflows");
        }
    }
}
