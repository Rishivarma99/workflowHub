namespace WorkflowHub.Common.Responses;

public static class ApiResponseFactory
{
    public static ApiResponse<T> Ok<T>(T data) =>
        new() { Success = true, Data = data, Error = null };

    public static ApiResponse<object?> Fail(string code, string message) =>
        new() { Success = false, Data = null, Error = new ApiError { Code = code, Message = message } };
}
