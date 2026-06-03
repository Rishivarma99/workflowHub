using WorkflowHub.Application.Auth.Models;
using WorkflowHub.Application.CQRS.Abstractions;

namespace WorkflowHub.Application.Features.Auth.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(
    Guid UserId,
    string? DisplayName,
    string? Username,
    string? Role,
    string? Team,
    string? Bio) : ICommand<AuthUserDto>;
