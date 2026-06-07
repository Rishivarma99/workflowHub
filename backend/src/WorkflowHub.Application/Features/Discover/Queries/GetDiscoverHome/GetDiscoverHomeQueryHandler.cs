using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Discover.ReadModels;
using WorkflowHub.Application.Features.Discover.Services;

namespace WorkflowHub.Application.Features.Discover.Queries.GetDiscoverHome;

public sealed class GetDiscoverHomeQueryHandler(DiscoverWorkflowListService listService)
    : IQueryHandler<GetDiscoverHomeQuery, DiscoverHomeDto>
{
    private const int TrendingLimit = 3;
    private const int RecentLimit = 3;

    public async Task<DiscoverHomeDto> Handle(GetDiscoverHomeQuery query, CancellationToken cancellationToken)
    {
        // DbContext is scoped per request and is not safe for concurrent queries.
        var (trending, _) = await listService.LoadPageAsync(
            query.UserId,
            page: 1,
            pageSize: TrendingLimit,
            sortBy: "downloads",
            componentTypes: [],
            cancellationToken);

        var (recent, _) = await listService.LoadPageAsync(
            query.UserId,
            page: 1,
            pageSize: RecentLimit,
            sortBy: "createdAt",
            componentTypes: [],
            cancellationToken);

        return new DiscoverHomeDto(trending, recent);
    }
}
