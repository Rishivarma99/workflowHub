using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkflowHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowCodeAndBuiltForAgents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "BuiltForAgents",
                table: "workflows",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkflowCode",
                table: "workflows",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE workflows
                SET "BuiltForAgents" = ARRAY["SourceIde"]
                WHERE "BuiltForAgents" IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE workflows
                SET "WorkflowCode" = 'WF-' || UPPER(SUBSTRING(REPLACE("Id"::text, '-', ''), 1, 5))
                WHERE "WorkflowCode" IS NULL OR "WorkflowCode" = '';
                """);

            migrationBuilder.AlterColumn<string[]>(
                name: "BuiltForAgents",
                table: "workflows",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(string[]),
                oldType: "text[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WorkflowCode",
                table: "workflows",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflows_WorkflowCode",
                table: "workflows",
                column: "WorkflowCode",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workflows_WorkflowCode",
                table: "workflows");

            migrationBuilder.DropColumn(
                name: "BuiltForAgents",
                table: "workflows");

            migrationBuilder.DropColumn(
                name: "WorkflowCode",
                table: "workflows");
        }
    }
}
