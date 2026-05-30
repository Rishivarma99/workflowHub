namespace WorkflowHub.Api.Extensions;

/// <summary>
/// Registers presentation concerns (controllers, presentation-only filters)
/// and composes the presentation HTTP pipeline.
/// </summary>
public static class PresentationExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers();

        return services;
    }

    public static WebApplication UsePresentationPipeline(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.MapControllers();

        return app;
    }
}
