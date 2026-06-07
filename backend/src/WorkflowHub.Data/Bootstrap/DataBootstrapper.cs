using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkflowHub.Data.Abstractions.Persistence;
using WorkflowHub.Data.Abstractions.Repositories;
using WorkflowHub.Data.Persistence;
using WorkflowHub.Data.Persistence.Repositories;
using WorkflowHub.Data.Persistence.Seeding;
namespace WorkflowHub.Data.Bootstrap;

public static class DataBootstrapper
{
    public static IServiceCollection Register(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, Persistence.UnitOfWork.UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserIdentityRepository, UserIdentityRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IWorkflowRepository, WorkflowRepository>();
        services.AddScoped<LearnContentSeedService>();
        services.AddScoped<DatabaseSeedService>();

        return services;
    }
}
