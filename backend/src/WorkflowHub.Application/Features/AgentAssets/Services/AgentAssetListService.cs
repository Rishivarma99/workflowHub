using Microsoft.EntityFrameworkCore;
using WorkflowHub.Application.Features.AgentAssets.ReadModels;
using WorkflowHub.Application.Features.Discover.ReadModels;
using WorkflowHub.Data.Entities;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Features.AgentAssets.Services;

public sealed class AgentAssetListService(AppDbContext dbContext)
{
    private static readonly string[] AllAssetTypes = ["rule", "command", "subagent", "hook", "skill"];

    public async Task<(IReadOnlyList<AgentAssetCardDto> Items, int Total)> LoadPageAsync(
        Guid? userId,
        int page,
        int pageSize,
        string sortBy,
        IReadOnlyList<string> assetTypes,
        CancellationToken cancellationToken)
    {
        var componentsQuery = dbContext.WorkflowComponents
            .AsNoTracking()
            .Include(c => c.Workflow)
            .ThenInclude(w => w.Owner)
            .AsQueryable();

        if (assetTypes.Count > 0)
        {
            componentsQuery = componentsQuery.Where(c => assetTypes.Contains(c.ComponentType));
        }

        componentsQuery = ApplySort(componentsQuery, sortBy);

        var total = await componentsQuery.CountAsync(cancellationToken);

        var components = await componentsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var starredIds = await LoadStarredComponentIdsAsync(userId, components, cancellationToken);

        var items = components
            .Select(c => MapAssetCard(c, starredIds.Contains(c.Id)))
            .ToList();

        return (items, total);
    }

    public static IReadOnlyList<string> ParseAssetTypes(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? []
            : raw
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => t.ToLowerInvariant())
                .Where(AllAssetTypes.Contains)
                .Distinct()
                .ToList();

    private async Task<HashSet<Guid>> LoadStarredComponentIdsAsync(
        Guid? userId,
        IReadOnlyList<WorkflowComponent> components,
        CancellationToken cancellationToken)
    {
        if (userId is null || components.Count == 0)
        {
            return [];
        }

        var componentIds = components.Select(c => c.Id).ToList();
        var starredIds = await dbContext.WorkflowComponentStars
            .AsNoTracking()
            .Where(s => s.UserId == userId && componentIds.Contains(s.WorkflowComponentId))
            .Select(s => s.WorkflowComponentId)
            .ToListAsync(cancellationToken);

        return starredIds.ToHashSet();
    }

    private static IQueryable<WorkflowComponent> ApplySort(
        IQueryable<WorkflowComponent> query,
        string sortBy) =>
        sortBy switch
        {
            "createdAt" => query
                .OrderByDescending(c => c.Workflow.UpdatedAtUtc)
                .ThenBy(c => c.Title),
            _ => query
                .OrderByDescending(c => c.StarCount)
                .ThenByDescending(c => c.Workflow.UpdatedAtUtc)
                .ThenBy(c => c.Title)
        };

    private static AgentAssetCardDto MapAssetCard(WorkflowComponent component, bool isStarred)
    {
        var capability = component.Capabilities.FirstOrDefault()
            ?? new ComponentCapabilityEntry { Name = component.Title, Description = component.Summary };

        return new AgentAssetCardDto(
            component.Id,
            component.Title,
            string.IsNullOrWhiteSpace(capability.Description) ? component.Summary : capability.Description,
            component.ComponentType,
            component.Path,
            component.WorkflowId,
            component.Workflow.Name,
            component.Workflow.SourceIde,
            component.StarCount,
            isStarred,
            new WorkflowAuthorDto(
                component.Workflow.Owner.Name,
                component.Workflow.Owner.DisplayName,
                component.Workflow.Owner.AvatarUrl));
    }
}
