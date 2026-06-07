using WorkflowHub.Api.Middleware;

namespace WorkflowHub.Api.Extensions;

public static class ExceptionMiddlewareExtensions
{
    public static WebApplication UseExceptionMiddleware(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        return app;
    }
}
