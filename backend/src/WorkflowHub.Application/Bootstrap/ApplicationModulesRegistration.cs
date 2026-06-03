using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkflowHub.Data.Bootstrap;

namespace WorkflowHub.Application.Bootstrap;

public static class ApplicationModulesRegistration
{
    public static IServiceCollection RegisterApplicationModules(
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
