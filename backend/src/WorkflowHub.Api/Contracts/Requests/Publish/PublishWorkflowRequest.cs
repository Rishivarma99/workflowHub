namespace WorkflowHub.Api.Contracts.Requests.Publish;

public sealed class PublishWorkflowRequest
{
    public string RepositoryUrl { get; set; } = string.Empty;
    public List<string> BuiltForAgents { get; set; } = [];
    public string SourceIde { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public List<PublishDependencyRequest> Dependencies { get; set; } = [];
    public string Complexity { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public PublishAnalysisRequest Analysis { get; set; } = new();
}
