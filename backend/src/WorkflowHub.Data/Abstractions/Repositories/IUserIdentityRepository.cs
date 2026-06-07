using WorkflowHub.Data.Entities;

namespace WorkflowHub.Data.Abstractions.Repositories;

public interface IUserIdentityRepository
{
    Task<UserIdentity?> GetByProviderAndSubWithUserAsync(
        string provider,
        string providerSub,
        CancellationToken cancellationToken = default);

    void Add(UserIdentity identity);
}
