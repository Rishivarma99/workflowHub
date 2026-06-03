using WorkflowHub.Application.Auth.Models;

namespace WorkflowHub.Application.Auth.Services;

public interface IExternalIdentityVerifier
{
    string Provider { get; }

    Task<ExternalIdentityClaims> VerifyAsync(string idToken, CancellationToken cancellationToken = default);
}
