using WorkflowHub.Common.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WorkflowHub.Api.Filters;

public sealed class ApiResponseFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (!ShouldWrap(context.Result))
        {
            return;
        }

        var objectResult = (ObjectResult)context.Result;
        var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;

        context.Result = new ObjectResult(new ApiResponse<object>
        {
            Success = true,
            Data = objectResult.Value,
            Error = null
        })
        {
            StatusCode = statusCode
        };
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private static bool ShouldWrap(IActionResult result)
    {
        if (result is not ObjectResult objectResult)
        {
            return false;
        }

        var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;

        return statusCode is >= 200 and < 300
               && objectResult.Value is not null
               && !IsAlreadyWrapped(objectResult.Value);
    }

    private static bool IsAlreadyWrapped(object value)
    {
        var type = value.GetType();

        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(ApiResponse<>);
    }
}
