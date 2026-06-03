using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using WorkflowHub.Application.Auth.Models;
using WorkflowHub.Application.Auth.Options;
using WorkflowHub.Common.Errors;
using WorkflowHub.Common.Exceptions;

namespace WorkflowHub.Application.Auth.Services;

public sealed class GoogleTokenVerifier(IOptions<GoogleOptions> options) : IExternalIdentityVerifier
{
    public string Provider => AuthProviders.Google;

    public async Task<ExternalIdentityClaims> VerifyAsync(
        string idToken,
        CancellationToken cancellationToken = default)
    {
        var clientId = options.Value.ClientId;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new InvalidOperationException("Google:ClientId is not configured.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings { Audience = [clientId] });

            if (string.IsNullOrWhiteSpace(payload.Subject))
            {
                throw new AppException(Errors.Auth.ExternalAuthFailed);
            }

            if (string.IsNullOrWhiteSpace(payload.Email))
            {
                throw new AppException(Errors.Auth.ExternalAuthFailed);
            }

            return new ExternalIdentityClaims(
                payload.Subject,
                payload.Email,
                payload.Name ?? payload.Email,
                payload.Picture);
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new AppException(Errors.Auth.ExternalAuthFailed);
        }
    }
}
