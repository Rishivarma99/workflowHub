using WorkflowHub.Application.CQRS.Abstractions;

namespace WorkflowHub.Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand(string? RefreshToken) : ICommand<bool>;
