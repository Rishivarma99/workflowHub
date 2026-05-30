namespace WorkflowHub.Api.Extensions;

/// <summary>
/// Registers API-project services (token services, auth services, seed services).
/// Stub only: registrations are added as those services land. Must not register
/// data / business bootstrappers.
/// </summary>
public static class ApiServicesExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        return services;
    }
}
