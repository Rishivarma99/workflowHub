using Microsoft.EntityFrameworkCore;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Learn.ReadModels;
using WorkflowHub.Data.Entities;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Features.Learn.Queries.GetArticleBySlug;

public sealed class GetArticleBySlugQueryHandler(AppDbContext dbContext)
    : IQueryHandler<GetArticleBySlugQuery, ArticleDetailDto?>
{
    public async Task<ArticleDetailDto?> Handle(
        GetArticleBySlugQuery query,
        CancellationToken cancellationToken)
    {
        var slug = query.Slug.Trim().ToLowerInvariant();
        if (slug.Length == 0)
        {
            return null;
        }

        return await dbContext.Articles
            .AsNoTracking()
            .Where(a => a.Slug == slug && a.Status == ArticleStatus.Published)
            .Select(a => new ArticleDetailDto(
                a.Id,
                a.Slug,
                a.Title,
                a.Summary,
                a.Content,
                a.ContentType.ToString().ToLowerInvariant(),
                new ArticleCategoryDto(
                    a.Category.Id,
                    a.Category.Name,
                    a.Category.Slug,
                    a.Category.Description),
                new ArticleAuthorDto(a.Author.Name, a.Author.DisplayName),
                a.IsPinned,
                a.PublishedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
