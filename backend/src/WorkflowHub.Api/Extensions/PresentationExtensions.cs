using WorkflowHub.Api.Filters;

namespace WorkflowHub.Api.Extensions;

/// <summary>
/// Registers presentation concerns (controllers, presentation-only filters)
/// and composes the presentation HTTP pipeline.
/// </summary>
public static class PresentationExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ApiResponseFilter>();
        });

        return services;
    }

    public static WebApplication UsePresentationPipeline(this WebApplication app)
    {
        app.UseExceptionMiddleware();
        app.UseHttpsRedirection();
        app.UseCors("Frontend");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}
