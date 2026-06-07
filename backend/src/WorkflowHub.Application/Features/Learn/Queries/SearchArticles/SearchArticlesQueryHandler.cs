using Microsoft.EntityFrameworkCore;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Learn.ReadModels;
using WorkflowHub.Application.Features.Learn.Services;
using WorkflowHub.Common.Responses;
using WorkflowHub.Data.Entities;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Features.Learn.Queries.SearchArticles;

public sealed class SearchArticlesQueryHandler(AppDbContext dbContext)
    : IQueryHandler<SearchArticlesQuery, PagedResult<ArticleCardDto>>
{
    public async Task<PagedResult<ArticleCardDto>> Handle(
        SearchArticlesQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);
        var terms = LearnSearchHelper.Tokenize(query.Query);
        var categorySlug = ParseCategorySlug(query.Filters);

        var articlesQuery = dbContext.Articles
            .AsNoTracking()
            .Where(a => a.Status == ArticleStatus.Published)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            articlesQuery = articlesQuery.Where(a => a.Category.Slug == categorySlug);
        }

        if (terms.Count > 0)
        {
            foreach (var term in terms)
            {
                var pattern = $"%{term}%";
                articlesQuery = articlesQuery.Where(a =>
                    EF.Functions.ILike(a.SearchText, pattern)
                    || EF.Functions.ILike(a.Title, pattern)
                    || EF.Functions.ILike(a.Summary, pattern)
                    || EF.Functions.ILike(a.Category.Name, pattern));
            }
        }

        articlesQuery = articlesQuery
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.PublishedAtUtc ?? a.CreatedAtUtc);

        var total = await articlesQuery.CountAsync(cancellationToken);

        var items = await articlesQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ArticleCardDto(
                a.Id,
                a.Slug,
                a.Title,
                a.Summary,
                a.ContentType.ToString().ToLowerInvariant(),
                a.Category.Name,
                a.Category.Slug,
                a.IsPinned,
                a.PublishedAtUtc,
                new ArticleAuthorDto(a.Author.Name, a.Author.DisplayName)))
            .ToListAsync(cancellationToken);

        return new PagedResult<ArticleCardDto>(items, total, page, pageSize);
    }

    private static string? ParseCategorySlug(IReadOnlyDictionary<string, string>? filters)
    {
        if (filters is null
            || !filters.TryGetValue("category", out var raw)
            || string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return raw.Trim().ToLowerInvariant();
    }
}
