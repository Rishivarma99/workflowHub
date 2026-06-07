using Microsoft.EntityFrameworkCore;
using WorkflowHub.Data.Abstractions.Repositories;
using WorkflowHub.Data.Entities;

namespace WorkflowHub.Data.Persistence.Repositories;

public sealed class UserIdentityRepository(AppDbContext dbContext) : IUserIdentityRepository
{
    public Task<UserIdentity?> GetByProviderAndSubWithUserAsync(
        string provider,
        string providerSub,
        CancellationToken cancellationToken = default) =>
        dbContext.UserIdentities
            .Include(i => i.User)
            .FirstOrDefaultAsync(
                i => i.Provider == provider && i.ProviderSub == providerSub,
                cancellationToken);

    public void Add(UserIdentity identity) => dbContext.UserIdentities.Add(identity);
}
