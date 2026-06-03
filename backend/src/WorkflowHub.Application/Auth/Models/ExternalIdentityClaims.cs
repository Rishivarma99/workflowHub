namespace WorkflowHub.Application.Auth.Models;

public sealed record ExternalIdentityClaims(
    string Sub,
    string Email,
    string Name,
    string? Picture);
