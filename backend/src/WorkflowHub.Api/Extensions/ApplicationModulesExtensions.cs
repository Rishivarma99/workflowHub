using WorkflowHub.Application.Bootstrap;
using WorkflowHub.Data.Bootstrap;

namespace WorkflowHub.Api.Extensions;

/// <summary>
/// Registers internal backend modules by resolving the connection string and
/// delegating to each layer's bootstrapper. Must not register API-project
/// services directly.
/// </summary>
public static class ApplicationModulesExtensions
{
    public static IServiceCollection AddApplicationModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing.");

        DataBootstrapper.Register(services, connectionString);
        BusinessBootstrapper.Register(services);

        return services;
    }
}
