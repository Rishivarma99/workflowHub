using Microsoft.EntityFrameworkCore.Storage;
using WorkflowHub.Application.Contracts.Persistence;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Persistence;

public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        dbContext.Database.BeginTransactionAsync(cancellationToken);
}
