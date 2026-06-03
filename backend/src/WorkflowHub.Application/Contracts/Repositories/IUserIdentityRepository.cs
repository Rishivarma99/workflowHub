using WorkflowHub.Data.Entities;

namespace WorkflowHub.Application.Contracts.Repositories;

public interface IUserIdentityRepository
{
    Task<UserIdentity?> GetByProviderAndSubWithUserAsync(
        string provider,
        string providerSub,
        CancellationToken cancellationToken = default);

    void Add(UserIdentity identity);
}
