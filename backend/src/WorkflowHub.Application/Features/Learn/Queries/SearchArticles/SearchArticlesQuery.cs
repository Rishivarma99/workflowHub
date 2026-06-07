using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Learn.ReadModels;
using WorkflowHub.Common.Responses;

namespace WorkflowHub.Application.Features.Learn.Queries.SearchArticles;

public sealed record SearchArticlesQuery(
    string? Query,
    int Page,
    int PageSize,
    IReadOnlyDictionary<string, string>? Filters) : IQuery<PagedResult<ArticleCardDto>>;
