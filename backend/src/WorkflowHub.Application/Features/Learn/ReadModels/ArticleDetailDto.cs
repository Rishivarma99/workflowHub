namespace WorkflowHub.Application.Features.Learn.ReadModels;

public sealed record ArticleDetailDto(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string Content,
    string ContentType,
    ArticleCategoryDto Category,
    ArticleAuthorDto Author,
    bool IsPinned,
    DateTime? PublishedAtUtc);
