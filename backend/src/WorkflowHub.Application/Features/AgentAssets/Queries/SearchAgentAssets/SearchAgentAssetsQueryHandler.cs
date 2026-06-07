using Microsoft.EntityFrameworkCore;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.AgentAssets.ReadModels;
using WorkflowHub.Application.Features.Discover.ReadModels;
using WorkflowHub.Application.Features.Discover.Services;
using WorkflowHub.Common.Errors;
using WorkflowHub.Common.Exceptions;
using WorkflowHub.Common.Responses;
using WorkflowHub.Data.Entities;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Features.AgentAssets.Queries.SearchAgentAssets;

public sealed class SearchAgentAssetsQueryHandler(AppDbContext dbContext)
    : IQueryHandler<SearchAgentAssetsQuery, PagedResult<AgentAssetCardDto>>
{
    private static readonly string[] AllAssetTypes = ["rule", "command", "subagent", "hook", "skill"];

    public async Task<PagedResult<AgentAssetCardDto>> Handle(
        SearchAgentAssetsQuery query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.Query))
        {
            throw new AppException(Errors.Search.QueryRequired);
        }

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);
        var terms = DiscoverSearchHelper.Tokenize(query.Query);
        var typeFilter = ParseAssetTypes(query.Filters);
        var sortBy = ResolveSortBy(query.SortBy, terms.Count > 0);

        var componentsQuery = dbContext.WorkflowComponents
            .AsNoTracking()
            .Include(c => c.Workflow)
            .ThenInclude(w => w.Owner)
            .AsQueryable();

        if (terms.Count > 0)
        {
            var matchingIds = await DiscoverSearchHelper.FindMatchingComponentIdsAsync(
                dbContext,
                terms,
                cancellationToken);

            componentsQuery = componentsQuery.Where(c => matchingIds.Contains(c.Id));
        }

        if (typeFilter.Count > 0)
        {
            componentsQuery = componentsQuery.Where(c => typeFilter.Contains(c.ComponentType));
        }

        componentsQuery = ApplySort(componentsQuery, sortBy);

        var total = await componentsQuery.CountAsync(cancellationToken);

        var components = await componentsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var starredIds = await LoadStarredComponentIdsAsync(query.UserId, components, cancellationToken);

        var items = components
            .Select(c => MapAssetCard(c, starredIds.Contains(c.Id)))
            .ToList();

        return new PagedResult<AgentAssetCardDto>(items, total, page, pageSize);
    }

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

    private static string ResolveSortBy(string? sortBy, bool hasQuery) =>
        sortBy?.Trim().ToLowerInvariant() switch
        {
            "stars" or "downloads" => "stars",
            "createdat" => "createdAt",
            "relevance" => "relevance",
            _ => hasQuery ? "relevance" : "stars"
        };

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

    private static IReadOnlyList<string> ParseAssetTypes(IReadOnlyDictionary<string, string>? filters)
    {
        if (filters is null
            || !filters.TryGetValue("assetTypes", out var raw)
            || string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.ToLowerInvariant())
            .Where(AllAssetTypes.Contains)
            .Distinct()
            .ToList();
    }

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
