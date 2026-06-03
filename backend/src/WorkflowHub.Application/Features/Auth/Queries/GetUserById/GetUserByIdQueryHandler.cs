using WorkflowHub.Application.Auth;
using WorkflowHub.Application.Auth.Models;
using WorkflowHub.Application.Contracts.Repositories;
using WorkflowHub.Application.CQRS.Abstractions;

namespace WorkflowHub.Application.Features.Auth.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler(IUserRepository userRepository)
    : IQueryHandler<GetUserByIdQuery, AuthUserDto?>
{
    public async Task<AuthUserDto?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(query.UserId, cancellationToken);
        return user is null ? null : AuthUserMapper.ToDto(user);
    }
}
