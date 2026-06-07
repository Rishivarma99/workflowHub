namespace WorkflowHub.Api.Contracts.Requests.Publish;

public sealed class PublishDependencyRequest
{
    public string Kind { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Requirement { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}
