using Microsoft.EntityFrameworkCore;
using WorkflowHub.Application.Features.Discover.ReadModels;
using WorkflowHub.Data.Entities;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Features.Discover.Services;

public sealed class DiscoverWorkflowListService(AppDbContext dbContext)
{
    private static readonly string[] AllComponentTypes = ["rule", "command", "subagent", "hook", "skill"];

    public async Task<(IReadOnlyList<WorkflowCardDto> Items, int Total)> LoadPageAsync(
        Guid? userId,
        int page,
        int pageSize,
        string sortBy,
        IReadOnlyList<string> componentTypes,
        CancellationToken cancellationToken)
    {
        var workflowsQuery = dbContext.Workflows
            .AsNoTracking()
            .Include(w => w.Owner)
            .Include(w => w.Components)
            .AsQueryable();

        workflowsQuery = ApplyComponentTypeFilter(workflowsQuery, componentTypes);
        workflowsQuery = ApplySort(workflowsQuery, sortBy);

        var total = await workflowsQuery.CountAsync(cancellationToken);

        var workflows = await workflowsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var starredIds = await LoadStarredWorkflowIdsAsync(userId, workflows, cancellationToken);

        var items = workflows
            .Select(w => MapWorkflowCard(w, starredIds.Contains(w.Id)))
            .ToList();

        return (items, total);
    }

    public static IReadOnlyList<string> ParseComponentTypes(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? []
            : raw
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => t.ToLowerInvariant())
                .Where(AllComponentTypes.Contains)
                .Distinct()
                .ToList();

    private async Task<HashSet<Guid>> LoadStarredWorkflowIdsAsync(
        Guid? userId,
        IReadOnlyList<Workflow> workflows,
        CancellationToken cancellationToken)
    {
        if (userId is null || workflows.Count == 0)
        {
            return [];
        }

        var workflowIds = workflows.Select(w => w.Id).ToList();
        var starredIds = await dbContext.WorkflowStars
            .AsNoTracking()
            .Where(s => s.UserId == userId && workflowIds.Contains(s.WorkflowId))
            .Select(s => s.WorkflowId)
            .ToListAsync(cancellationToken);

        return starredIds.ToHashSet();
    }

    private static IQueryable<Workflow> ApplySort(IQueryable<Workflow> query, string sortBy) =>
        sortBy switch
        {
            "createdAt" => query
                .OrderByDescending(w => w.UpdatedAtUtc ?? w.CreatedAtUtc)
                .ThenByDescending(w => w.DownloadCount),
            _ => query
                .OrderByDescending(w => w.DownloadCount)
                .ThenByDescending(w => w.UpdatedAtUtc ?? w.CreatedAtUtc)
        };

    private static IQueryable<Workflow> ApplyComponentTypeFilter(
        IQueryable<Workflow> query,
        IReadOnlyList<string> typeFilter)
    {
        if (typeFilter.Count == 0)
        {
            return query;
        }

        return query.Where(w => w.Components.Any(c => typeFilter.Contains(c.ComponentType)));
    }

    private static WorkflowCardDto MapWorkflowCard(Workflow workflow, bool isStarred) =>
        new(
            workflow.Id,
            workflow.Name,
            workflow.Description,
            workflow.Tags,
            workflow.WorkflowCode,
            workflow.BuiltForAgents,
            workflow.SourceIde,
            workflow.StarCount,
            workflow.DownloadCount,
            isStarred,
            workflow.ComponentTypes,
            workflow.Components
                .GroupBy(c => c.ComponentType)
                .ToDictionary(g => g.Key, g => g.Count()),
            new WorkflowAuthorDto(workflow.Owner.Name, workflow.Owner.DisplayName, workflow.Owner.AvatarUrl),
            null);
}
