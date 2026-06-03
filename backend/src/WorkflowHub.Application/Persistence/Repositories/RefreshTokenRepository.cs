using Microsoft.EntityFrameworkCore;
using WorkflowHub.Application.Contracts.Repositories;
using WorkflowHub.Data.Entities;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Persistence.Repositories;

public sealed class RefreshTokenRepository(AppDbContext dbContext) : IRefreshTokenRepository
{
    public void Add(RefreshToken refreshToken) => dbContext.RefreshTokens.Add(refreshToken);

    public Task<RefreshToken?> GetActiveByTokenHashWithUserAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(
                rt => rt.TokenHash == tokenHash
                    && rt.RevokedAtUtc == null
                    && !rt.IsUsed
                    && rt.ExpiresAtUtc > DateTime.UtcNow,
                cancellationToken);
}
