using Microsoft.EntityFrameworkCore;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Engagement.ReadModels;
using WorkflowHub.Common.Errors;
using WorkflowHub.Common.Exceptions;
using WorkflowHub.Data.Entities;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Features.Engagement.Commands.StarAgentAsset;

public sealed class StarAgentAssetCommandHandler(AppDbContext dbContext)
    : ICommandHandler<StarAgentAssetCommand, StarActionResultDto>
{
    public async Task<StarActionResultDto> Handle(
        StarAgentAssetCommand command,
        CancellationToken cancellationToken)
    {
        var component = await dbContext.WorkflowComponents
            .FirstOrDefaultAsync(c => c.Id == command.ComponentId, cancellationToken);

        if (component is null)
        {
            throw new AppException(Errors.AgentAsset.NotFound);
        }

        var existing = await dbContext.WorkflowComponentStars
            .FirstOrDefaultAsync(
                s => s.UserId == command.UserId && s.WorkflowComponentId == command.ComponentId,
                cancellationToken);

        if (existing is not null)
        {
            return new StarActionResultDto(true, component.StarCount);
        }

        dbContext.WorkflowComponentStars.Add(new WorkflowComponentStar
        {
            Id = Guid.NewGuid(),
            WorkflowComponentId = command.ComponentId,
            UserId = command.UserId,
            CreatedAtUtc = DateTime.UtcNow
        });

        component.StarCount++;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new StarActionResultDto(true, component.StarCount);
    }
}
