namespace WorkflowHub.Application.Features.MyWorkflows.ReadModels;

public sealed record MyWorkflowListItemDto(
    Guid Id,
    string Name,
    string Description,
    string SourceIde,
    int StarCount,
    int DownloadCount,
    int ComponentCount,
    DateTime UpdatedAtUtc);

public sealed record MyWorkflowsSummaryDto(
    IReadOnlyList<MyWorkflowListItemDto> Items,
    int PublishedCount,
    int TotalDownloads,
    int TotalComponentsIndexed);
