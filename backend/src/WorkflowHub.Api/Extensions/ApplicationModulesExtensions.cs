using WorkflowHub.Application.Bootstrap;
using WorkflowHub.Data.Bootstrap;

namespace WorkflowHub.Api.Extensions;

public static class ApplicationModulesExtensions
{
    public static IServiceCollection AddApplicationModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing.");

        DataBootstrapper.Register(services, connectionString);
        BusinessBootstrapper.Register(services, configuration);

        return services;
    }
}
