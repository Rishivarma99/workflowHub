using Microsoft.EntityFrameworkCore;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Engagement.ReadModels;
using WorkflowHub.Common.Errors;
using WorkflowHub.Common.Exceptions;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Features.Engagement.Commands.UnstarWorkflow;

public sealed class UnstarWorkflowCommandHandler(AppDbContext dbContext)
    : ICommandHandler<UnstarWorkflowCommand, StarActionResultDto>
{
    public async Task<StarActionResultDto> Handle(
        UnstarWorkflowCommand command,
        CancellationToken cancellationToken)
    {
        var workflow = await dbContext.Workflows
            .FirstOrDefaultAsync(w => w.Id == command.WorkflowId, cancellationToken);

        if (workflow is null)
        {
            throw new AppException(Errors.Workflow.NotFound);
        }

        var existing = await dbContext.WorkflowStars
            .FirstOrDefaultAsync(
                s => s.UserId == command.UserId && s.WorkflowId == command.WorkflowId,
                cancellationToken);

        if (existing is null)
        {
            return new StarActionResultDto(false, workflow.StarCount);
        }

        dbContext.WorkflowStars.Remove(existing);
        workflow.StarCount = Math.Max(0, workflow.StarCount - 1);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new StarActionResultDto(false, workflow.StarCount);
    }
}
