using FluentValidation;
using WorkflowHub.Application.Features.Publish.Services;

namespace WorkflowHub.Application.Features.Publish.Commands.PublishWorkflow;

public sealed class PublishWorkflowCommandValidator : AbstractValidator<PublishWorkflowCommand>
{
    private static readonly HashSet<string> AllowedSourceIdes = ["cursor", "claude", "copilot"];
    private static readonly HashSet<string> AllowedComplexities = ["beginner", "intermediate", "advanced"];
    private static readonly HashSet<string> AllowedAudiences =
    [
        "frontend", "backend", "fullstack", "devops", "ai-engineering", "general"
    ];

    public PublishWorkflowCommandValidator()
    {
        RuleFor(c => c.OwnerId).NotEmpty();
        RuleFor(c => c.RepositoryUrl)
            .NotEmpty()
            .Must(url => GitHubUrlParser.TryParse(url) is not null)
            .WithMessage("Repository URL must be a valid GitHub URL.");
        RuleFor(c => c.BuiltForAgents)
            .NotEmpty()
            .Must(agents => agents.Count > 0)
            .WithMessage("At least one target agent is required.");
        RuleForEach(c => c.BuiltForAgents)
            .Must(ide => AllowedSourceIdes.Contains(ide.Trim().ToLowerInvariant()));
        RuleFor(c => c.WorkflowName).NotEmpty().MinimumLength(3).MaximumLength(120);
        RuleFor(c => c.Description).NotEmpty().MinimumLength(10).MaximumLength(2000);
        RuleFor(c => c.Tags).NotEmpty();
        RuleFor(c => c.Complexity)
            .NotEmpty()
            .Must(c => AllowedComplexities.Contains(c.Trim().ToLowerInvariant()));
        RuleFor(c => c.TargetAudience)
            .NotEmpty()
            .Must(a => AllowedAudiences.Contains(a.Trim().ToLowerInvariant()));
        RuleFor(c => c.Analysis).NotNull();
        RuleForEach(c => c.Dependencies).ChildRules(dep =>
        {
            dep.RuleFor(d => d.Kind).Must(k => k is "mcp" or "plugin");
            dep.RuleFor(d => d.Name).NotEmpty().MaximumLength(120);
            dep.RuleFor(d => d.Requirement).Must(r => r is "required" or "optional");
            dep.RuleFor(d => d.Note).MaximumLength(240);
        });
    }
}
