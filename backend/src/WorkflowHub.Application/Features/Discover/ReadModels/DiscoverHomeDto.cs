namespace WorkflowHub.Application.Features.Discover.ReadModels;

public sealed record DiscoverHomeDto(
    IReadOnlyList<WorkflowCardDto> Trending,
    IReadOnlyList<WorkflowCardDto> Recent);
