using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Engagement.ReadModels;

namespace WorkflowHub.Application.Features.Engagement.Commands.StarWorkflow;

public sealed record StarWorkflowCommand(Guid UserId, Guid WorkflowId)
    : ICommand<StarActionResultDto>, ITransactionalCommand;
