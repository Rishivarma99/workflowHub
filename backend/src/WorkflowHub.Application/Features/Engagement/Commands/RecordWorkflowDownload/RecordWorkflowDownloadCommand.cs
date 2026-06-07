using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Engagement.ReadModels;

namespace WorkflowHub.Application.Features.Engagement.Commands.RecordWorkflowDownload;

public sealed record RecordWorkflowDownloadCommand(Guid UserId, Guid WorkflowId)
    : ICommand<RecordWorkflowDownloadResultDto>, ITransactionalCommand;
