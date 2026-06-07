using Microsoft.EntityFrameworkCore;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Engagement.ReadModels;
using WorkflowHub.Common.Errors;
using WorkflowHub.Common.Exceptions;
using WorkflowHub.Data.Entities;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Features.Engagement.Commands.StarWorkflow;

public sealed class StarWorkflowCommandHandler(AppDbContext dbContext)
    : ICommandHandler<StarWorkflowCommand, StarActionResultDto>
{
    public async Task<StarActionResultDto> Handle(
        StarWorkflowCommand command,
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

        if (existing is not null)
        {
            return new StarActionResultDto(true, workflow.StarCount);
        }

        dbContext.WorkflowStars.Add(new WorkflowStar
        {
            Id = Guid.NewGuid(),
            WorkflowId = command.WorkflowId,
            UserId = command.UserId,
            CreatedAtUtc = DateTime.UtcNow
        });

        workflow.StarCount++;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new StarActionResultDto(true, workflow.StarCount);
    }
}
