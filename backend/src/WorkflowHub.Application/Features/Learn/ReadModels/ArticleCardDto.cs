namespace WorkflowHub.Application.Features.Learn.ReadModels;

public sealed record ArticleAuthorDto(string Name, string? DisplayName);

public sealed record ArticleCardDto(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string ContentType,
    string CategoryName,
    string CategorySlug,
    bool IsPinned,
    DateTime? PublishedAtUtc,
    ArticleAuthorDto Author);
