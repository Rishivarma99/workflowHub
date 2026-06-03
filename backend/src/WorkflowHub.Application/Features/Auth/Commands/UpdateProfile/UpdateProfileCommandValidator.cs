using FluentValidation;

namespace WorkflowHub.Application.Features.Auth.Commands.UpdateProfile;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        When(x => x.DisplayName is not null, () =>
        {
            RuleFor(x => x.DisplayName!)
                .MinimumLength(2)
                .MaximumLength(50);
        });

        When(x => x.Username is not null, () =>
        {
            RuleFor(x => x.Username!)
                .MinimumLength(3)
                .MaximumLength(30)
                .Matches("^[a-z0-9_]+$")
                .WithMessage("Username may only contain lowercase letters, numbers, and underscores.");
        });

        When(x => x.Role is not null, () =>
        {
            RuleFor(x => x.Role!).MaximumLength(80);
        });

        When(x => x.Team is not null, () =>
        {
            RuleFor(x => x.Team!).MaximumLength(80);
        });

        When(x => x.Bio is not null, () =>
        {
            RuleFor(x => x.Bio!).MaximumLength(160);
        });
    }
}
