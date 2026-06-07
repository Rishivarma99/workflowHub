namespace WorkflowHub.Api.Contracts.Requests.Publish;

public sealed class PublishAnalysisRequest
{
    public string WorkflowName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public List<string> SuggestedDependencies { get; set; } = [];
    public List<PublishAnalysisComponentRequest> Components { get; set; } = [];
}
