using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Discover.ReadModels;
using WorkflowHub.Common.Responses;

namespace WorkflowHub.Application.Features.Discover.Queries.SearchWorkflows;

public sealed record SearchWorkflowsQuery(
    Guid? UserId,
    string? Query,
    int Page,
    int PageSize,
    string? SortBy,
    IReadOnlyDictionary<string, string>? Filters) : IQuery<PagedResult<WorkflowCardDto>>;
