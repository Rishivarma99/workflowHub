using Microsoft.EntityFrameworkCore.Storage;
using WorkflowHub.Data.Abstractions.Persistence;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Data.Persistence.UnitOfWork;

public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        dbContext.Database.BeginTransactionAsync(cancellationToken);
}
