using WorkflowHub.Application.Auth.Services;
using WorkflowHub.Data.Abstractions.Persistence;
using WorkflowHub.Application.CQRS.Abstractions;

namespace WorkflowHub.Application.Features.Auth.Commands.Logout;

public sealed class LogoutCommandHandler(
    IRefreshTokenService refreshTokenService,
    IUnitOfWork unitOfWork) : ICommandHandler<LogoutCommand, bool>
{
    public async Task<bool> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return true;
        }

        var stored = await refreshTokenService.FindActiveByPlainTokenAsync(
            command.RefreshToken,
            cancellationToken);

        if (stored is not null)
        {
            refreshTokenService.Revoke(stored);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}
