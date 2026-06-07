using Microsoft.EntityFrameworkCore;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Learn.ReadModels;
using WorkflowHub.Data.Entities;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Features.Learn.Queries.GetLearnHome;

public sealed class GetLearnHomeQueryHandler(AppDbContext dbContext)
    : IQueryHandler<GetLearnHomeQuery, LearnHomeDto>
{
    private const int PinnedLimit = 5;
    private const int LatestLimit = 6;

    public async Task<LearnHomeDto> Handle(GetLearnHomeQuery query, CancellationToken cancellationToken)
    {
        var categories = await dbContext.ArticleCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new ArticleCategoryDto(c.Id, c.Name, c.Slug, c.Description))
            .ToListAsync(cancellationToken);

        var pinnedGuides = await LoadPublishedArticlesAsync(
            dbContext.Articles
                .AsNoTracking()
                .Where(a => a.IsPinned && a.ContentType == ArticleContentType.Guide)
                .OrderBy(a => a.Title)
                .Take(PinnedLimit),
            cancellationToken);

        var latestArticles = await LoadPublishedArticlesAsync(
            dbContext.Articles
                .AsNoTracking()
                .Where(a => !a.IsPinned)
                .OrderByDescending(a => a.PublishedAtUtc ?? a.CreatedAtUtc)
                .Take(LatestLimit),
            cancellationToken);

        return new LearnHomeDto(pinnedGuides, latestArticles, categories);
    }

    private static async Task<IReadOnlyList<ArticleCardDto>> LoadPublishedArticlesAsync(
        IQueryable<Article> query,
        CancellationToken cancellationToken) =>
        await query
            .Where(a => a.Status == ArticleStatus.Published)
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
}
