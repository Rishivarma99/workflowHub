using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.AgentAssets.ReadModels;
using WorkflowHub.Common.Responses;

namespace WorkflowHub.Application.Features.AgentAssets.Queries.SearchAgentAssets;

public sealed record SearchAgentAssetsQuery(
    Guid? UserId,
    string? Query,
    int Page,
    int PageSize,
    string? SortBy,
    IReadOnlyDictionary<string, string>? Filters) : IQuery<PagedResult<AgentAssetCardDto>>;
