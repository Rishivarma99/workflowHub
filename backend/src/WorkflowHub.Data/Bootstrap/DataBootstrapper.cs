using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Data.Bootstrap;

/// <summary>
/// Registers Data-layer services (DbContext, repositories, unit of work, seeding).
/// Stub only: registers the DbContext against Postgres. Repository and seed
/// registrations are added as features land.
/// </summary>
public static class DataBootstrapper
{
    public static IServiceCollection Register(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
