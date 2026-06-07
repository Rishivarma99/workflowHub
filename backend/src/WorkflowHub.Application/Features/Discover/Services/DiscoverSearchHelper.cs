using Microsoft.EntityFrameworkCore;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Features.Discover.Services;

internal static class DiscoverSearchHelper
{
    private static readonly HashSet<string> StopWords =
    [
        "a", "an", "the", "to", "of", "in", "on", "and", "or", "for", "with", "by", "from", "into", "is", "it", "my", "me"
    ];

    internal static IReadOnlyList<string> Tokenize(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        return query
            .Trim()
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => t.Length > 1 && !StopWords.Contains(t))
            .Distinct()
            .ToList();
    }

    internal static bool MatchesAnyTerm(string haystack, IReadOnlyList<string> terms)
    {
        if (terms.Count == 0)
        {
            return true;
        }

        var lower = haystack.ToLowerInvariant();
        return terms.Any(lower.Contains);
    }

    internal static async Task<HashSet<Guid>> FindMatchingWorkflowIdsAsync(
        AppDbContext dbContext,
        IReadOnlyList<string> terms,
        CancellationToken cancellationToken)
    {
        if (terms.Count == 0)
        {
            return [];
        }

        var ids = new HashSet<Guid>();
        foreach (var term in terms)
        {
            var pattern = $"%{term}%";
            var batch = await dbContext.Workflows
                .AsNoTracking()
                .Where(w =>
                    EF.Functions.ILike(w.SearchText, pattern)
                    || EF.Functions.ILike(w.Name, pattern)
                    || EF.Functions.ILike(w.Description, pattern)
                    || EF.Functions.ILike(w.Owner.Name, pattern)
                    || (w.Owner.DisplayName != null && EF.Functions.ILike(w.Owner.DisplayName, pattern)))
                .Select(w => w.Id)
                .ToListAsync(cancellationToken);

            foreach (var id in batch)
            {
                ids.Add(id);
            }
        }

        return ids;
    }

    internal static async Task<HashSet<Guid>> FindMatchingComponentIdsAsync(
        AppDbContext dbContext,
        IReadOnlyList<string> terms,
        CancellationToken cancellationToken)
    {
        if (terms.Count == 0)
        {
            return [];
        }

        var ids = new HashSet<Guid>();
        foreach (var term in terms)
        {
            var pattern = $"%{term}%";
            var batch = await dbContext.WorkflowComponents
                .AsNoTracking()
                .Where(c =>
                    EF.Functions.ILike(c.SearchText, pattern)
                    || EF.Functions.ILike(c.Title, pattern)
                    || EF.Functions.ILike(c.Summary, pattern)
                    || EF.Functions.ILike(c.Path, pattern))
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            foreach (var id in batch)
            {
                ids.Add(id);
            }
        }

        return ids;
    }

    internal static async Task<HashSet<Guid>> FindMatchingWorkflowIdsIncludingAssetsAsync(
        AppDbContext dbContext,
        IReadOnlyList<string> terms,
        CancellationToken cancellationToken)
    {
        var workflowIds = await FindMatchingWorkflowIdsAsync(dbContext, terms, cancellationToken);

        var componentIds = await FindMatchingComponentIdsAsync(dbContext, terms, cancellationToken);
        if (componentIds.Count == 0)
        {
            return workflowIds;
        }

        var parentIds = await dbContext.WorkflowComponents
            .AsNoTracking()
            .Where(c => componentIds.Contains(c.Id))
            .Select(c => c.WorkflowId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var id in parentIds)
        {
            workflowIds.Add(id);
        }

        return workflowIds;
    }

}
