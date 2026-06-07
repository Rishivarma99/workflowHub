using Microsoft.EntityFrameworkCore;
using WorkflowHub.Application.Auth;
using WorkflowHub.Application.Auth.Models;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Features.Auth.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler(AppDbContext dbContext)
    : IQueryHandler<GetUserByIdQuery, AuthUserDto?>
{
    public async Task<AuthUserDto?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == query.UserId, cancellationToken);

        return user is null ? null : AuthUserMapper.ToDto(user);
    }
}
