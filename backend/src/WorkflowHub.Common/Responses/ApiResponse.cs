namespace WorkflowHub.Common.Responses;

/// <summary>
/// Standard success response envelope: { success, data, error }.
/// See ai-rules/backend/02-api/api-response-contract-rules.md.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }

    public T? Data { get; set; }

    public ApiError? Error { get; set; }
}
