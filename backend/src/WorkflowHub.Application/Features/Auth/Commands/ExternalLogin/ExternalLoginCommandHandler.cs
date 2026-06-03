using WorkflowHub.Application.Auth;
using WorkflowHub.Application.Auth.Models;
using WorkflowHub.Application.Auth.Services;
using WorkflowHub.Application.Contracts.Persistence;
using WorkflowHub.Application.Contracts.Repositories;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Common.Errors;
using WorkflowHub.Common.Exceptions;
using WorkflowHub.Data.Entities;

namespace WorkflowHub.Application.Features.Auth.Commands.ExternalLogin;

public sealed class ExternalLoginCommandHandler(
    IEnumerable<IExternalIdentityVerifier> verifiers,
    IUserRepository userRepository,
    IUserIdentityRepository userIdentityRepository,
    IJwtTokenService jwtTokenService,
    IRefreshTokenService refreshTokenService,
    IUnitOfWork unitOfWork) : ICommandHandler<ExternalLoginCommand, AuthResult>
{
    public async Task<AuthResult> Handle(ExternalLoginCommand command, CancellationToken cancellationToken)
    {
        var normalizedProvider = command.Provider.Trim().ToLowerInvariant();
        var verifier = verifiers.FirstOrDefault(v =>
            string.Equals(v.Provider, normalizedProvider, StringComparison.OrdinalIgnoreCase));

        if (verifier is null)
        {
            throw new AppException(
                Errors.Auth.UnknownProvider,
                new Dictionary<string, object> { { "Provider", command.Provider } });
        }

        ExternalIdentityClaims claims;
        try
        {
            claims = await verifier.VerifyAsync(command.IdToken, cancellationToken);
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new AppException(Errors.Auth.ExternalAuthFailed);
        }

        var now = DateTime.UtcNow;
        var emailKey = claims.Email.Trim().ToLowerInvariant();

        var identity = await userIdentityRepository.GetByProviderAndSubWithUserAsync(
            normalizedProvider,
            claims.Sub,
            cancellationToken);

        User user;
        if (identity is not null)
        {
            user = identity.User;
            user.Email = claims.Email;
            user.Name = claims.Name;
            user.AvatarUrl = claims.Picture;
            user.UpdatedAtUtc = now;

            identity.ProviderEmail = claims.Email;
            identity.ProviderAvatarUrl = claims.Picture;
            identity.LastLoginAtUtc = now;
        }
        else
        {
            var existingUser = await userRepository.GetByEmailAsync(emailKey, cancellationToken);

            if (existingUser is null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = claims.Email,
                    Name = claims.Name,
                    AvatarUrl = claims.Picture,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                };
                userRepository.Add(user);
            }
            else
            {
                user = existingUser;
                user.Email = claims.Email;
                user.Name = claims.Name;
                user.AvatarUrl = claims.Picture;
                user.UpdatedAtUtc = now;
            }

            userIdentityRepository.Add(new UserIdentity
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Provider = normalizedProvider,
                ProviderSub = claims.Sub,
                ProviderEmail = claims.Email,
                ProviderAvatarUrl = claims.Picture,
                CreatedAtUtc = now,
                LastLoginAtUtc = now
            });
        }

        var (plainRefresh, _) = await refreshTokenService.CreateAsync(user, command.DeviceInfo, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var access = jwtTokenService.CreateAccessToken(user);
        return new AuthResult(
            new TokenPair(access, plainRefresh),
            AuthUserMapper.ToDto(user));
    }
}
