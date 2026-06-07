namespace WorkflowHub.Application.Features.Learn.ReadModels;

public sealed record ArticleCategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string Description);

public sealed record LearnHomeDto(
    IReadOnlyList<ArticleCardDto> PinnedGuides,
    IReadOnlyList<ArticleCardDto> LatestArticles,
    IReadOnlyList<ArticleCategoryDto> Categories);
