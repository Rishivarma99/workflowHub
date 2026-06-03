namespace WorkflowHub.Api.Contracts.Requests.Auth;

public sealed record ExternalLoginRequest(string Provider, string IdToken);
