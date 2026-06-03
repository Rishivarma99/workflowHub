using Microsoft.EntityFrameworkCore;
using WorkflowHub.Application.Contracts.Repositories;
using WorkflowHub.Data.Entities;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Persistence.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(
            u => u.Email.ToLower() == normalizedEmail,
            cancellationToken);

    public Task<User?> GetByUsernameAsync(string normalizedUsername, CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(
            u => u.Username != null && u.Username.ToLower() == normalizedUsername,
            cancellationToken);

    public void Add(User user) => dbContext.Users.Add(user);
}
