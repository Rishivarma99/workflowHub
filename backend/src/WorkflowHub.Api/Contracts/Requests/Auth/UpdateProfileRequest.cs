namespace WorkflowHub.Api.Contracts.Requests.Auth;

public sealed record UpdateProfileRequest(
    string? DisplayName,
    string? Username,
    string? Role,
    string? Team,
    string? Bio);
