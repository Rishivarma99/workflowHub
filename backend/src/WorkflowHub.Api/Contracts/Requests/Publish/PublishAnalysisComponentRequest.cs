namespace WorkflowHub.Api.Contracts.Requests.Publish;

public sealed class PublishAnalysisComponentRequest
{
    public string Path { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<PublishAnalysisCapabilityRequest> Capabilities { get; set; } = [];
    public List<string> Keywords { get; set; } = [];
    public List<string> SearchPhrases { get; set; } = [];
    public List<string> Technologies { get; set; } = [];
    public List<string> Dependencies { get; set; } = [];
}
