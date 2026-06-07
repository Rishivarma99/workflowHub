using Microsoft.EntityFrameworkCore;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Discover.ReadModels;
using WorkflowHub.Application.Features.Discover.Services;
using WorkflowHub.Common.Errors;
using WorkflowHub.Common.Exceptions;
using WorkflowHub.Common.Responses;
using WorkflowHub.Data.Entities;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Features.Discover.Queries.SearchWorkflows;

public sealed class SearchWorkflowsQueryHandler(AppDbContext dbContext)
    : IQueryHandler<SearchWorkflowsQuery, PagedResult<WorkflowCardDto>>
{
    private static readonly string[] AllComponentTypes = ["rule", "command", "subagent", "hook", "skill"];

    public async Task<PagedResult<WorkflowCardDto>> Handle(
        SearchWorkflowsQuery query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.Query))
        {
            throw new AppException(Errors.Search.QueryRequired);
        }

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);
        var terms = DiscoverSearchHelper.Tokenize(query.Query);
        var typeFilter = ParseComponentTypes(query.Filters);
        var sortBy = ResolveSortBy(query.SortBy, terms.Count > 0);

        var workflowsQuery = dbContext.Workflows
            .AsNoTracking()
            .Include(w => w.Owner)
            .Include(w => w.Components)
            .AsQueryable();

        workflowsQuery = ApplyComponentTypeFilter(workflowsQuery, typeFilter);

        if (terms.Count > 0)
        {
            var matchingIds = await DiscoverSearchHelper.FindMatchingWorkflowIdsIncludingAssetsAsync(
                dbContext,
                terms,
                cancellationToken);

            workflowsQuery = workflowsQuery.Where(w => matchingIds.Contains(w.Id));
        }

        workflowsQuery = ApplySort(workflowsQuery, sortBy);

        var total = await workflowsQuery.CountAsync(cancellationToken);

        var workflows = await workflowsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var starredIds = await LoadStarredWorkflowIdsAsync(query.UserId, workflows, cancellationToken);

        var items = workflows
            .Select(w => MapWorkflowCard(
                w,
                terms.Count > 0 ? FindMatchedComponent(w, terms) : null,
                starredIds.Contains(w.Id)))
            .ToList();

        return new PagedResult<WorkflowCardDto>(items, total, page, pageSize);
    }

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

    private static string ResolveSortBy(string? sortBy, bool hasQuery) =>
        sortBy?.Trim().ToLowerInvariant() switch
        {
            "downloads" => "downloads",
            "createdat" => "createdAt",
            "relevance" => "relevance",
            _ => hasQuery ? "relevance" : "downloads"
        };

    private static IQueryable<Workflow> ApplySort(IQueryable<Workflow> query, string sortBy) =>
        sortBy switch
        {
            "createdAt" => query
                .OrderByDescending(w => w.UpdatedAtUtc ?? w.CreatedAtUtc)
                .ThenByDescending(w => w.DownloadCount),
            "relevance" => query
                .OrderByDescending(w => w.DownloadCount)
                .ThenByDescending(w => w.UpdatedAtUtc ?? w.CreatedAtUtc),
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

    private static IReadOnlyList<string> ParseComponentTypes(IReadOnlyDictionary<string, string>? filters)
    {
        if (filters is null
            || !filters.TryGetValue("componentTypes", out var raw)
            || string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.ToLowerInvariant())
            .Where(AllComponentTypes.Contains)
            .Distinct()
            .ToList();
    }

    private static MatchedComponentDto? FindMatchedComponent(Workflow workflow, IReadOnlyList<string> terms)
    {
        foreach (var component in workflow.Components)
        {
            if (!ComponentMatchesTerms(component, terms))
            {
                continue;
            }

            var capability = PickMatchedCapability(component, terms);
            return new MatchedComponentDto(
                component.Id,
                component.ComponentType,
                component.Path,
                component.Title,
                capability.Name);
        }

        return null;
    }

    private static bool ComponentMatchesTerms(WorkflowComponent component, IReadOnlyList<string> terms) =>
        DiscoverSearchHelper.MatchesAnyTerm(component.SearchText, terms)
        || DiscoverSearchHelper.MatchesAnyTerm(component.Title, terms)
        || DiscoverSearchHelper.MatchesAnyTerm(component.Summary, terms)
        || component.Capabilities.Any(cap =>
            terms.Any(term =>
                DiscoverSearchHelper.MatchesAnyTerm(cap.Name, [term])
                || DiscoverSearchHelper.MatchesAnyTerm(cap.Description, [term])));

    private static ComponentCapabilityEntry PickMatchedCapability(
        WorkflowComponent component,
        IReadOnlyList<string> terms)
    {
        if (terms.Count == 0 || component.Capabilities.Count == 0)
        {
            return component.Capabilities.FirstOrDefault()
                ?? new ComponentCapabilityEntry { Name = component.Title, Description = component.Summary };
        }

        var match = component.Capabilities.FirstOrDefault(cap =>
            terms.Any(term =>
                DiscoverSearchHelper.MatchesAnyTerm(cap.Name, [term])
                || DiscoverSearchHelper.MatchesAnyTerm(cap.Description, [term])));

        return match ?? component.Capabilities[0];
    }

    private static WorkflowCardDto MapWorkflowCard(
        Workflow workflow,
        MatchedComponentDto? matched,
        bool isStarred) =>
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
            matched);
}
