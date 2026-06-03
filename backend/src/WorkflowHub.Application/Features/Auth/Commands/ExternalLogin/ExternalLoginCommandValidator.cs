using FluentValidation;

namespace WorkflowHub.Application.Features.Auth.Commands.ExternalLogin;

public sealed class ExternalLoginCommandValidator : AbstractValidator<ExternalLoginCommand>
{
    public ExternalLoginCommandValidator()
    {
        RuleFor(x => x.Provider).NotEmpty();
        RuleFor(x => x.IdToken).NotEmpty();
    }
}
