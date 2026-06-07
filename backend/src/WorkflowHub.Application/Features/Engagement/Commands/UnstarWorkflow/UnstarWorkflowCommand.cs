using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Engagement.ReadModels;

namespace WorkflowHub.Application.Features.Engagement.Commands.UnstarWorkflow;

public sealed record UnstarWorkflowCommand(Guid UserId, Guid WorkflowId)
    : ICommand<StarActionResultDto>, ITransactionalCommand;
