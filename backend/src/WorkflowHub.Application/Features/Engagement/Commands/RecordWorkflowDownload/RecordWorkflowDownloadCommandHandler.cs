using Microsoft.EntityFrameworkCore;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Engagement.ReadModels;
using WorkflowHub.Common.Errors;
using WorkflowHub.Common.Exceptions;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Features.Engagement.Commands.RecordWorkflowDownload;

public sealed class RecordWorkflowDownloadCommandHandler(AppDbContext dbContext)
    : ICommandHandler<RecordWorkflowDownloadCommand, RecordWorkflowDownloadResultDto>
{
    public async Task<RecordWorkflowDownloadResultDto> Handle(
        RecordWorkflowDownloadCommand command,
        CancellationToken cancellationToken)
    {
        var workflow = await dbContext.Workflows
            .FirstOrDefaultAsync(w => w.Id == command.WorkflowId, cancellationToken);

        if (workflow is null)
        {
            throw new AppException(Errors.Workflow.NotFound);
        }

        workflow.DownloadCount++;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RecordWorkflowDownloadResultDto(workflow.Id, workflow.DownloadCount);
    }
}
