namespace WorkflowHub.Application.Auth;

/// <summary>
/// External auth provider ids stored in <c>user_identities.provider</c>.
/// To add a provider: constant here, <see cref="Services.IExternalIdentityVerifier"/>, appsettings, DI — no DB migration.
/// </summary>
public static class AuthProviders
{
    public const string Google = "google";
}
