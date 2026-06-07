using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.AgentAssets.ReadModels;
using WorkflowHub.Common.Responses;

namespace WorkflowHub.Application.Features.AgentAssets.Queries.BrowseAgentAssets;

public sealed record BrowseAgentAssetsQuery(
    Guid? UserId,
    int Page,
    int PageSize,
    string? AssetTypes) : IQuery<PagedResult<AgentAssetCardDto>>;
