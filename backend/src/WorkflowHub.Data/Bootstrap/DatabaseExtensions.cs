using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Data.Bootstrap;

public static class DatabaseExtensions
{
    public static WebApplication ApplyDevelopmentMigrations(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        return app;
    }
}
