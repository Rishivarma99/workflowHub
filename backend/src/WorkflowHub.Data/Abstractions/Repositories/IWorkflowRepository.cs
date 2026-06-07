using WorkflowHub.Data.Entities;

namespace WorkflowHub.Data.Abstractions.Repositories;

public interface IWorkflowRepository
{
    Task<bool> NameExistsAsync(string normalizedName, CancellationToken cancellationToken = default);

    Task<bool> WorkflowCodeExistsAsync(string workflowCode, CancellationToken cancellationToken = default);

    Task<Workflow?> GetByNameAsync(string normalizedName, CancellationToken cancellationToken = default);

    Task<Workflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Workflow?> GetByIdWithComponentsAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(Workflow workflow);
}
