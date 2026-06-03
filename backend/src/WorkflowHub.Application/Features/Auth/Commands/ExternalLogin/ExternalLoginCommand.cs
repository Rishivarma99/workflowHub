using WorkflowHub.Application.Auth.Models;
using WorkflowHub.Application.CQRS.Abstractions;

namespace WorkflowHub.Application.Features.Auth.Commands.ExternalLogin;

public sealed record ExternalLoginCommand(
    string Provider,
    string IdToken,
    string? DeviceInfo) : ICommand<AuthResult>;
