using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Engagement.ReadModels;

namespace WorkflowHub.Application.Features.Engagement.Commands.UnstarAgentAsset;

public sealed record UnstarAgentAssetCommand(Guid UserId, Guid ComponentId)
    : ICommand<StarActionResultDto>, ITransactionalCommand;
