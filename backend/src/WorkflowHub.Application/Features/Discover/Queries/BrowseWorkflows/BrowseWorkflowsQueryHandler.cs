using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Discover.ReadModels;
using WorkflowHub.Application.Features.Discover.Services;
using WorkflowHub.Common.Responses;

namespace WorkflowHub.Application.Features.Discover.Queries.BrowseWorkflows;

public sealed class BrowseWorkflowsQueryHandler(DiscoverWorkflowListService listService)
    : IQueryHandler<BrowseWorkflowsQuery, PagedResult<WorkflowCardDto>>
{
    public async Task<PagedResult<WorkflowCardDto>> Handle(
        BrowseWorkflowsQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);
        var componentTypes = DiscoverWorkflowListService.ParseComponentTypes(query.ComponentTypes);

        var (items, total) = await listService.LoadPageAsync(
            query.UserId,
            page,
            pageSize,
            sortBy: "downloads",
            componentTypes,
            cancellationToken);

        return new PagedResult<WorkflowCardDto>(items, total, page, pageSize);
    }
}
