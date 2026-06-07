using Microsoft.EntityFrameworkCore;
using WorkflowHub.Data.Abstractions.Repositories;
using WorkflowHub.Data.Entities;

namespace WorkflowHub.Data.Persistence.Repositories;

public sealed class WorkflowRepository(AppDbContext dbContext) : IWorkflowRepository
{
    public Task<bool> NameExistsAsync(string normalizedName, CancellationToken cancellationToken = default) =>
        dbContext.Workflows.AnyAsync(
            w => w.Name.ToLower() == normalizedName,
            cancellationToken);

    public Task<bool> WorkflowCodeExistsAsync(string workflowCode, CancellationToken cancellationToken = default) =>
        dbContext.Workflows.AnyAsync(
            w => w.WorkflowCode.ToUpper() == workflowCode.ToUpper(),
            cancellationToken);

    public Task<Workflow?> GetByNameAsync(string normalizedName, CancellationToken cancellationToken = default) =>
        dbContext.Workflows.FirstOrDefaultAsync(
            w => w.Name.ToLower() == normalizedName,
            cancellationToken);

    public Task<Workflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Workflows.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public Task<Workflow?> GetByIdWithComponentsAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Workflows
            .Include(w => w.Components)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public void Add(Workflow workflow) => dbContext.Workflows.Add(workflow);
}
