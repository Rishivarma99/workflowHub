using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Install.ReadModels;
using WorkflowHub.Application.Features.Install.Services;
using WorkflowHub.Common.Errors;
using WorkflowHub.Common.Exceptions;
using WorkflowHub.Data.Abstractions.Repositories;

namespace WorkflowHub.Application.Features.Install.Commands.GenerateInstallPrompt;

public sealed class GenerateInstallPromptCommandHandler(IWorkflowRepository workflowRepository)
    : ICommandHandler<GenerateInstallPromptCommand, GenerateInstallPromptResultDto>
{
    public async Task<GenerateInstallPromptResultDto> Handle(
        GenerateInstallPromptCommand command,
        CancellationToken cancellationToken)
    {
        var workflow = await workflowRepository.GetByIdAsync(command.WorkflowId, cancellationToken);
        if (workflow is null)
        {
            throw new AppException(Errors.Workflow.NotFound);
        }

        var prompt = InstallPromptService.BuildPrompt(
            workflow,
            command.TargetAgent.Trim().ToLowerInvariant(),
            command.InstallLevel.Trim().ToLowerInvariant());

        return new GenerateInstallPromptResultDto(prompt);
    }
}
