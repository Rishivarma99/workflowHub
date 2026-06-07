using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Discover.ReadModels;
using WorkflowHub.Common.Responses;

namespace WorkflowHub.Application.Features.Discover.Queries.BrowseWorkflows;

public sealed record BrowseWorkflowsQuery(
    Guid? UserId,
    int Page,
    int PageSize,
    string? ComponentTypes) : IQuery<PagedResult<WorkflowCardDto>>;
