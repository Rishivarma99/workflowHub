using Microsoft.EntityFrameworkCore;
using WorkflowHub.Data.Entities;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Data.Persistence.Seeding;

public sealed class LearnContentSeedService(AppDbContext dbContext)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.ArticleCategories.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var author = await EnsureSystemAuthorAsync(now, cancellationToken);
        var categories = CreateCategories(now);
        dbContext.ArticleCategories.AddRange(categories);
        await dbContext.SaveChangesAsync(cancellationToken);

        var categoryBySlug = categories.ToDictionary(c => c.Slug);
        var articles = LearnSeedContent.Articles
            .Select(seed => CreateArticle(seed, categoryBySlug[seed.CategorySlug], author.Id, now))
            .ToList();

        dbContext.Articles.AddRange(articles);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<User> EnsureSystemAuthorAsync(DateTime now, CancellationToken cancellationToken)
    {
        var existing = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == LearnSeedIds.SystemAuthorId, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var author = new User
        {
            Id = LearnSeedIds.SystemAuthorId,
            Email = "learn@workflowhub.local",
            Name = "WorkflowHub Team",
            DisplayName = "WorkflowHub Team",
            CreatedAtUtc = now
        };

        dbContext.Users.Add(author);
        await dbContext.SaveChangesAsync(cancellationToken);
        return author;
    }

    private static List<ArticleCategory> CreateCategories(DateTime now) =>
        LearnSeedContent.Categories
            .Select(c => new ArticleCategory
            {
                Id = Guid.NewGuid(),
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description
            })
            .ToList();

    private static Article CreateArticle(
        LearnSeedContent.ArticleSeed seed,
        ArticleCategory category,
        Guid authorId,
        DateTime now)
    {
        var contentType = seed.ContentType.Equals("guide", StringComparison.OrdinalIgnoreCase)
            ? ArticleContentType.Guide
            : ArticleContentType.Article;

        return new Article
        {
            Id = Guid.NewGuid(),
            Slug = seed.Slug,
            Title = seed.Title,
            Summary = seed.Summary,
            Content = seed.Content.Trim(),
            CategoryId = category.Id,
            AuthorId = authorId,
            Status = ArticleStatus.Published,
            ContentType = contentType,
            IsPinned = seed.IsPinned,
            PublishedAtUtc = now,
            SearchText = ArticleSearchTextBuilder.Build(seed.Title, seed.Summary, seed.Content, category.Name),
            CreatedAtUtc = now,
            CreatedByUserId = authorId
        };
    }
}
