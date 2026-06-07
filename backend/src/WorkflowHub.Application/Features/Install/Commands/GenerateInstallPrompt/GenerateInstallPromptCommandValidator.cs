using FluentValidation;

namespace WorkflowHub.Application.Features.Install.Commands.GenerateInstallPrompt;

public sealed class GenerateInstallPromptCommandValidator : AbstractValidator<GenerateInstallPromptCommand>
{
    private static readonly HashSet<string> AllowedTargetAgents = ["same", "cursor", "claude", "copilot"];
    private static readonly HashSet<string> AllowedInstallLevels = ["project", "system"];

    public GenerateInstallPromptCommandValidator()
    {
        RuleFor(c => c.WorkflowId).NotEmpty();
        RuleFor(c => c.TargetAgent)
            .NotEmpty()
            .Must(agent => AllowedTargetAgents.Contains(agent.Trim().ToLowerInvariant()));
        RuleFor(c => c.InstallLevel)
            .NotEmpty()
            .Must(level => AllowedInstallLevels.Contains(level.Trim().ToLowerInvariant()));
    }
}
