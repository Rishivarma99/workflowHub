using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.AgentAssets.ReadModels;
using WorkflowHub.Application.Features.AgentAssets.Services;
using WorkflowHub.Common.Responses;

namespace WorkflowHub.Application.Features.AgentAssets.Queries.BrowseAgentAssets;

public sealed class BrowseAgentAssetsQueryHandler(AgentAssetListService listService)
    : IQueryHandler<BrowseAgentAssetsQuery, PagedResult<AgentAssetCardDto>>
{
    public async Task<PagedResult<AgentAssetCardDto>> Handle(
        BrowseAgentAssetsQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);
        var assetTypes = AgentAssetListService.ParseAssetTypes(query.AssetTypes);

        var (items, total) = await listService.LoadPageAsync(
            query.UserId,
            page,
            pageSize,
            sortBy: "stars",
            assetTypes,
            cancellationToken);

        return new PagedResult<AgentAssetCardDto>(items, total, page, pageSize);
    }
}
