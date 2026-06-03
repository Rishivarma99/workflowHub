using WorkflowHub.Application.Auth.Models;
using WorkflowHub.Application.Auth.Services;
using WorkflowHub.Application.Contracts.Persistence;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Common.Errors;
using WorkflowHub.Common.Exceptions;

namespace WorkflowHub.Application.Features.Auth.Commands.RefreshSession;

public sealed class RefreshSessionCommandHandler(
    IRefreshTokenService refreshTokenService,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork) : ICommandHandler<RefreshSessionCommand, TokenPair>
{
    public async Task<TokenPair> Handle(RefreshSessionCommand command, CancellationToken cancellationToken)
    {
        var stored = await refreshTokenService.FindActiveByPlainTokenAsync(
            command.RefreshToken,
            cancellationToken);

        if (stored is null)
        {
            throw new AppException(Errors.Auth.InvalidRefreshToken);
        }

        refreshTokenService.MarkUsed(stored);
        var user = stored.User;
        var (plainRefresh, _) = await refreshTokenService.CreateAsync(user, command.DeviceInfo, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var access = jwtTokenService.CreateAccessToken(user);
        return new TokenPair(access, plainRefresh);
    }
}
