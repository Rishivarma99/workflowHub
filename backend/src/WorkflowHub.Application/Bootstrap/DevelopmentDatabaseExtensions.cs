using Microsoft.AspNetCore.Builder;
using WorkflowHub.Data.Bootstrap;

namespace WorkflowHub.Application.Bootstrap;

public static class DevelopmentDatabaseExtensions
{
    public static WebApplication ApplyDevelopmentMigrations(this WebApplication app) =>
        DatabaseExtensions.ApplyDevelopmentMigrations(app);
}
