namespace WorkflowHub.Application.Auth.Models;

public sealed record TokenPair(string AccessToken, string RefreshToken);

public sealed record AuthUserDto(
    Guid Id,
    string Email,
    string Name,
    string? DisplayName,
    string? Username,
    string? Role,
    string? Team,
    string? Bio,
    string? AvatarUrl,
    DateTime CreatedAtUtc);

public sealed record AuthResult(TokenPair Tokens, AuthUserDto User);
