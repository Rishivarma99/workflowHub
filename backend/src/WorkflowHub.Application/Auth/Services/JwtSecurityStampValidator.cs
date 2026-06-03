using Microsoft.EntityFrameworkCore;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Auth.Services;

public interface IJwtSecurityStampValidator
{
    Task<bool> IsValidAsync(Guid userId, string securityStamp, CancellationToken cancellationToken = default);
}

public sealed class JwtSecurityStampValidator(AppDbContext db) : IJwtSecurityStampValidator
{
    public async Task<bool> IsValidAsync(
        Guid userId,
        string securityStamp,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user is not null && user.SecurityStamp == securityStamp;
    }
}
