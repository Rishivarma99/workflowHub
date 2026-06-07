using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Install.ReadModels;

namespace WorkflowHub.Application.Features.Install.Commands.GenerateInstallPrompt;

public sealed record GenerateInstallPromptCommand(
    Guid WorkflowId,
    string TargetAgent,
    string InstallLevel) : ICommand<GenerateInstallPromptResultDto>;
