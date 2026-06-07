namespace WorkflowHub.Api.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddFrontendCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy
                    .WithOrigins(
                        "http://localhost:4290",
                        "http://127.0.0.1:4290",
                        "http://localhost:4200",
                        "http://127.0.0.1:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}
