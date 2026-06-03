using WorkflowHub.Data.Entities;

namespace WorkflowHub.Application.Contracts.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<User?> GetByUsernameAsync(string normalizedUsername, CancellationToken cancellationToken = default);

    void Add(User user);
}
