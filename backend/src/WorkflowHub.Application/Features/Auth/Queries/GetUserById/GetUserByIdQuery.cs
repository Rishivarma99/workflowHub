using WorkflowHub.Application.Auth.Models;
using WorkflowHub.Application.CQRS.Abstractions;

namespace WorkflowHub.Application.Features.Auth.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<AuthUserDto?>;
