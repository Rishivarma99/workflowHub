using FluentValidation;
using WorkflowHub.Application.Features.Publish.Services;

namespace WorkflowHub.Application.Features.Publish.Queries.ValidateRepository;

public sealed class ValidateRepositoryQueryValidator : AbstractValidator<ValidateRepositoryQuery>
{
    public ValidateRepositoryQueryValidator()
    {
        RuleFor(q => q.RepositoryUrl)
            .NotEmpty()
            .Must(url => GitHubUrlParser.TryParse(url) is not null)
            .WithMessage("Repository URL must be a valid public GitHub URL.");
    }
}
