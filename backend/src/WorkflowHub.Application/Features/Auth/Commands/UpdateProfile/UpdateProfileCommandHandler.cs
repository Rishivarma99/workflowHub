using WorkflowHub.Application.Auth;
using WorkflowHub.Application.Auth.Models;
using WorkflowHub.Application.Contracts.Persistence;
using WorkflowHub.Application.Contracts.Repositories;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Common.Errors;
using WorkflowHub.Common.Exceptions;

namespace WorkflowHub.Application.Features.Auth.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateProfileCommand, AuthUserDto>
{
    public async Task<AuthUserDto> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            throw new AppException(Errors.Auth.UserNotFound);
        }

        if (command.Username is not null)
        {
            var usernameKey = command.Username.ToLowerInvariant();
            var taken = await userRepository.GetByUsernameAsync(usernameKey, cancellationToken);
            if (taken is not null && taken.Id != command.UserId)
            {
                throw new AppException(Errors.Auth.UsernameTaken);
            }

            user.Username = usernameKey;
        }
        else
        {
            user.Username = null;
        }

        user.DisplayName = command.DisplayName;
        user.Role = command.Role;
        user.Team = command.Team;
        user.Bio = command.Bio;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return AuthUserMapper.ToDto(user);
    }
}
