using WorkflowHub.Application.Features.Install.Services;
using WorkflowHub.Data.Abstractions.Persistence;
using WorkflowHub.Data.Abstractions.Repositories;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Publish.Services;
using WorkflowHub.Common.Errors;
using WorkflowHub.Common.Exceptions;

namespace WorkflowHub.Application.Features.Publish.Commands.PublishWorkflow;

public sealed class PublishWorkflowCommandHandler(
    IWorkflowRepository workflowRepository,
    IGitHubRepositoryClient gitHubClient,
    IUnitOfWork unitOfWork) : ICommandHandler<PublishWorkflowCommand, PublishWorkflowResultDto>
{
    public async Task<PublishWorkflowResultDto> Handle(
        PublishWorkflowCommand command,
        CancellationToken cancellationToken)
    {
        var normalizedName = command.WorkflowName.Trim().ToLowerInvariant();
        if (await workflowRepository.NameExistsAsync(normalizedName, cancellationToken))
        {
            throw new AppException(Errors.Workflow.NameTaken);
        }

        var analysisErrors = PublishWorkflowMapper.ValidateAnalysis(command.Analysis);
        if (analysisErrors.Count > 0)
        {
            throw new AppException(
                Errors.Workflow.InvalidAnalysis,
                new Dictionary<string, object> { ["errors"] = analysisErrors });
        }

        var repoStatus = await gitHubClient.InspectPublicRepositoryAsync(
            command.RepositoryUrl,
            cancellationToken);

        if (!repoStatus.Exists || !repoStatus.IsPublic || !repoStatus.Accessible)
        {
            throw new AppException(Errors.Workflow.RepositoryUnavailable);
        }

        if (repoStatus.Repository is null || string.IsNullOrWhiteSpace(repoStatus.DefaultBranchSha))
        {
            throw new AppException(Errors.Workflow.RepositoryUnavailable);
        }

        var workflow = PublishWorkflowMapper.MapToEntity(
            command.OwnerId,
            command.RepositoryUrl,
            repoStatus.DefaultBranchSha,
            command.BuiltForAgents,
            await GenerateUniqueWorkflowCodeAsync(cancellationToken),
            command.WorkflowName,
            command.Description,
            command.Tags,
            command.Dependencies,
            command.Complexity,
            command.TargetAudience,
            command.Analysis,
            repoStatus.Repository);

        workflowRepository.Add(workflow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new PublishWorkflowResultDto(workflow.Id, workflow.Name, workflow.WorkflowCode);
    }

    private async Task<string> GenerateUniqueWorkflowCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var code = WorkflowCodeGenerator.Generate();
            if (!await workflowRepository.WorkflowCodeExistsAsync(code, cancellationToken))
            {
                return code;
            }
        }

        throw new AppException(Errors.Internal.InternalServerError);
    }
}
