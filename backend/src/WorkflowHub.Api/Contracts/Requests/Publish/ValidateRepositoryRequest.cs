namespace WorkflowHub.Api.Contracts.Requests.Publish;

public sealed class ValidateRepositoryRequest
{
    public string RepositoryUrl { get; set; } = string.Empty;
}
