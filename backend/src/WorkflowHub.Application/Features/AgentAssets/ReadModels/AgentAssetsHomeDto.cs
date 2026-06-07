namespace WorkflowHub.Application.Features.AgentAssets.ReadModels;

public sealed record AgentAssetsHomeDto(
    IReadOnlyList<AgentAssetCardDto> Popular,
    IReadOnlyList<AgentAssetCardDto> Recent);
