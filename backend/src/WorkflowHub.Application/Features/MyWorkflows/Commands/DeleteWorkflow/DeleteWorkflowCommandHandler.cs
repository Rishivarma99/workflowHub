using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Common.Errors;
using WorkflowHub.Common.Exceptions;
using WorkflowHub.Data.Abstractions.Persistence;
using WorkflowHub.Data.Abstractions.Repositories;

namespace WorkflowHub.Application.Features.MyWorkflows.Commands.DeleteWorkflow;

public sealed class DeleteWorkflowCommandHandler(
    IWorkflowRepository workflowRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteWorkflowCommand, bool>
{
    public async Task<bool> Handle(DeleteWorkflowCommand command, CancellationToken cancellationToken)
    {
        var workflow = await workflowRepository.GetByIdWithComponentsAsync(command.WorkflowId, cancellationToken);
        if (workflow is null)
        {
            throw new AppException(Errors.Workflow.NotFound);
        }

        if (workflow.OwnerId != command.OwnerId)
        {
            throw new AppException(Errors.Workflow.Forbidden);
        }

        workflow.SoftDeleteWithComponents(command.OwnerId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
