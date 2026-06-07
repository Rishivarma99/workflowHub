using Microsoft.EntityFrameworkCore;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Engagement.ReadModels;
using WorkflowHub.Common.Errors;
using WorkflowHub.Common.Exceptions;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Features.Engagement.Commands.UnstarAgentAsset;

public sealed class UnstarAgentAssetCommandHandler(AppDbContext dbContext)
    : ICommandHandler<UnstarAgentAssetCommand, StarActionResultDto>
{
    public async Task<StarActionResultDto> Handle(
        UnstarAgentAssetCommand command,
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

        if (existing is null)
        {
            return new StarActionResultDto(false, component.StarCount);
        }

        dbContext.WorkflowComponentStars.Remove(existing);
        component.StarCount = Math.Max(0, component.StarCount - 1);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new StarActionResultDto(false, component.StarCount);
    }
}
