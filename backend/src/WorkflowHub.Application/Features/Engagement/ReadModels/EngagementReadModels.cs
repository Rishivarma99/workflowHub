namespace WorkflowHub.Application.Features.Engagement.ReadModels;

public sealed record StarActionResultDto(bool IsStarred, int StarCount);

public sealed record RecordWorkflowDownloadResultDto(Guid WorkflowId, int DownloadCount);
