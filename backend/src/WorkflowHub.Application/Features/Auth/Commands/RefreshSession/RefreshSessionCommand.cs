using WorkflowHub.Application.Auth.Models;
using WorkflowHub.Application.CQRS.Abstractions;

namespace WorkflowHub.Application.Features.Auth.Commands.RefreshSession;

public sealed record RefreshSessionCommand(
    string RefreshToken,
    string? DeviceInfo) : ICommand<TokenPair>;
