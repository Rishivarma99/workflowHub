namespace WorkflowHub.Api.Extensions;

public static class ApiServicesExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddJwtAuthentication(configuration)
            .AddFrontendCors();
}
