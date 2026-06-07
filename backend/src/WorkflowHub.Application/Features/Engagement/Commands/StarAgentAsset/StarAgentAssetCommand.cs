using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Engagement.ReadModels;

namespace WorkflowHub.Application.Features.Engagement.Commands.StarAgentAsset;

public sealed record StarAgentAssetCommand(Guid UserId, Guid ComponentId)
    : ICommand<StarActionResultDto>, ITransactionalCommand;
