using WorkflowHub.Data.Bootstrap;

namespace WorkflowHub.Api.Extensions;

public static class SeedingExtensions
{
    public static WebApplication ApplyDevelopmentMigrations(this WebApplication app) =>
        DatabaseExtensions.ApplyDevelopmentMigrations(app);
}
