using WorkflowHub.Application.CQRS.Abstractions;

namespace WorkflowHub.Application.Features.MyWorkflows.Commands.DeleteWorkflow;

public sealed record DeleteWorkflowCommand(Guid OwnerId, Guid WorkflowId) : ICommand<bool>, ITransactionalCommand;
