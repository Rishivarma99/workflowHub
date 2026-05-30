namespace WorkflowHub.Common.Responses;

/// <summary>
/// Standard error payload carried inside <see cref="ApiResponse{T}"/>.
/// Produced by central exception middleware, never by controllers.
/// </summary>
public class ApiError
{
    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
