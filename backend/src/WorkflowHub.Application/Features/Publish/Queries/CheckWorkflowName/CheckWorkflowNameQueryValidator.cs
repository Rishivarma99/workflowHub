using FluentValidation;

namespace WorkflowHub.Application.Features.Publish.Queries.CheckWorkflowName;

public sealed class CheckWorkflowNameQueryValidator : AbstractValidator<CheckWorkflowNameQuery>
{
    public CheckWorkflowNameQueryValidator()
    {
        RuleFor(q => q.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(120);
    }
}
