using WorkflowHub.Application.Bootstrap;

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
        IConfiguration configuration) =>
        services.RegisterApplicationModules(configuration);
}
