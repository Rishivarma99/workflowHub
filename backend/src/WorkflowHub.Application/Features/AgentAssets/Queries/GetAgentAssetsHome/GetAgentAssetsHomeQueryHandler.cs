using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.AgentAssets.ReadModels;
using WorkflowHub.Application.Features.AgentAssets.Services;

namespace WorkflowHub.Application.Features.AgentAssets.Queries.GetAgentAssetsHome;

public sealed class GetAgentAssetsHomeQueryHandler(AgentAssetListService listService)
    : IQueryHandler<GetAgentAssetsHomeQuery, AgentAssetsHomeDto>
{
    private const int SectionLimit = 6;

    public async Task<AgentAssetsHomeDto> Handle(
        GetAgentAssetsHomeQuery query,
        CancellationToken cancellationToken)
    {
        // DbContext is scoped per request and is not safe for concurrent queries.
        var (popular, _) = await listService.LoadPageAsync(
            query.UserId,
            page: 1,
            pageSize: SectionLimit,
            sortBy: "stars",
            assetTypes: [],
            cancellationToken);

        var (recent, _) = await listService.LoadPageAsync(
            query.UserId,
            page: 1,
            pageSize: SectionLimit,
            sortBy: "createdAt",
            assetTypes: [],
            cancellationToken);

        return new AgentAssetsHomeDto(popular, recent);
    }
}
