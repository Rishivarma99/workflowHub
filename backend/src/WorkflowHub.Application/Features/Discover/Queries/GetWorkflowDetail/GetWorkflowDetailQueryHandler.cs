using Microsoft.EntityFrameworkCore;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Discover.ReadModels;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Features.Discover.Queries.GetWorkflowDetail;

public sealed class GetWorkflowDetailQueryHandler(AppDbContext dbContext)
    : IQueryHandler<GetWorkflowDetailQuery, WorkflowDetailDto?>
{
    public async Task<WorkflowDetailDto?> Handle(
        GetWorkflowDetailQuery query,
        CancellationToken cancellationToken)
    {
        var workflow = await dbContext.Workflows
            .AsNoTracking()
            .Include(w => w.Owner)
            .Include(w => w.Components)
            .FirstOrDefaultAsync(w => w.Id == query.WorkflowId, cancellationToken);

        if (workflow is null)
        {
            return null;
        }

        var isStarred = query.UserId is Guid userId
            && await dbContext.WorkflowStars
                .AsNoTracking()
                .AnyAsync(
                    s => s.UserId == userId && s.WorkflowId == query.WorkflowId,
                    cancellationToken);

        var starredComponentIds = await LoadStarredComponentIdsAsync(
            query.UserId,
            workflow.Components.Select(c => c.Id).ToList(),
            cancellationToken);

        return new WorkflowDetailDto(
            workflow.Id,
            workflow.Name,
            workflow.Description,
            workflow.Tags,
            workflow.RepositoryUrl,
            workflow.CommitSha,
            workflow.WorkflowCode,
            workflow.BuiltForAgents,
            workflow.SourceIde,
            workflow.Complexity,
            workflow.TargetAudience,
            workflow.StarCount,
            workflow.DownloadCount,
            isStarred,
            workflow.UpdatedAtUtc ?? workflow.CreatedAtUtc,
            new WorkflowAuthorDto(workflow.Owner.Name, workflow.Owner.DisplayName, workflow.Owner.AvatarUrl),
            workflow.Dependencies
                .Select(d => new WorkflowDependencyDetailDto(d.Kind, d.Name, d.Requirement, d.Note))
                .ToList(),
            workflow.Components
                .OrderBy(c => c.Path)
                .Select(c => new WorkflowComponentDetailDto(
                    c.Id,
                    c.Path,
                    c.GitHubUrl,
                    c.ComponentType,
                    c.Title,
                    c.Summary,
                    c.StarCount,
                    starredComponentIds.Contains(c.Id),
                    c.Capabilities
                        .Select(cap => new WorkflowCapabilityDto(cap.Name, cap.Description))
                        .ToList(),
                    c.Keywords,
                    c.SearchPhrases,
                    c.Technologies,
                    c.Dependencies))
                .ToList());
    }

    private async Task<HashSet<Guid>> LoadStarredComponentIdsAsync(
        Guid? userId,
        IReadOnlyList<Guid> componentIds,
        CancellationToken cancellationToken)
    {
        if (userId is null || componentIds.Count == 0)
        {
            return [];
        }

        var starredIds = await dbContext.WorkflowComponentStars
            .AsNoTracking()
            .Where(s => s.UserId == userId && componentIds.Contains(s.WorkflowComponentId))
            .Select(s => s.WorkflowComponentId)
            .ToListAsync(cancellationToken);

        return starredIds.ToHashSet();
    }
}
