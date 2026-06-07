using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkflowHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowComponentSearchText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                table: "workflow_components",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE workflow_components
                SET "SearchText" = trim(both E'\n' from concat_ws(E'\n',
                    "Title",
                    "Summary",
                    "Path",
                    "ComponentType",
                    array_to_string("Keywords", ' '),
                    array_to_string("SearchPhrases", ' '),
                    array_to_string("Technologies", ' '),
                    COALESCE((
                        SELECT string_agg(
                            coalesce(elem->>'name', '') || E'\n' || coalesce(elem->>'description', ''),
                            E'\n'
                        )
                        FROM jsonb_array_elements("Capabilities") AS elem
                    ), '')
                ));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SearchText",
                table: "workflow_components");
        }
    }
}
