using Microsoft.Extensions.DependencyInjection;

namespace WorkflowHub.Application.Bootstrap;

/// <summary>
/// Registers Application-layer services (CQRS dispatchers, pipeline behaviors,
/// handlers, application services). Stub only: registrations are added as
/// CQRS components and features land.
/// </summary>
public static class BusinessBootstrapper
{
    public static IServiceCollection Register(IServiceCollection services)
    {
        return services;
    }
}
