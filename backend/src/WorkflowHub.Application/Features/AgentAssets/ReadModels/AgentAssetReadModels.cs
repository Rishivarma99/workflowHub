using WorkflowHub.Application.Features.Discover.ReadModels;

namespace WorkflowHub.Application.Features.AgentAssets.ReadModels;

public sealed record AgentAssetCardDto(
    Guid Id,
    string Name,
    string Description,
    string AssetType,
    string Path,
    Guid WorkflowId,
    string WorkflowName,
    string SourceIde,
    int StarCount,
    bool IsStarred,
    WorkflowAuthorDto Author);
