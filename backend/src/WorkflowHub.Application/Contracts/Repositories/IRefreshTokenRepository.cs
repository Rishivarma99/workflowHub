using WorkflowHub.Data.Entities;

namespace WorkflowHub.Application.Contracts.Repositories;

public interface IRefreshTokenRepository
{
    void Add(RefreshToken refreshToken);

    Task<RefreshToken?> GetActiveByTokenHashWithUserAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);
}
