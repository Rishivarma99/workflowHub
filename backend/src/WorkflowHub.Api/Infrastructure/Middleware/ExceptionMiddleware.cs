using System.Text.Json;
using WorkflowHub.Common.Errors;
using WorkflowHub.Common.Exceptions;
using WorkflowHub.Common.Responses;

namespace WorkflowHub.Api.Infrastructure.Middleware;

public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            logger.LogWarning(
                "Business error: {Code}. Path: {Path}. TraceId: {TraceId}",
                ex.Error.Code,
                context.Request.Path,
                context.TraceIdentifier);

            await HandleAppExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unhandled exception. Path: {Path}. TraceId: {TraceId}",
                context.Request.Path,
                context.TraceIdentifier);

            await HandleGenericExceptionAsync(context);
        }
    }

    private static async Task HandleAppExceptionAsync(HttpContext context, AppException ex)
    {
        context.Response.StatusCode = ex.Error.StatusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            Success = false,
            Error = new ApiError
            {
                Code = ex.Error.Code,
                Message = ErrorFormatter.Format(ex.Error.MessageTemplate, ex.Params)
            }
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static async Task HandleGenericExceptionAsync(HttpContext context)
    {
        context.Response.StatusCode = Errors.Internal.InternalServerError.StatusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            Success = false,
            Error = new ApiError
            {
                Code = Errors.Internal.InternalServerError.Code,
                Message = Errors.Internal.InternalServerError.MessageTemplate
            }
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
