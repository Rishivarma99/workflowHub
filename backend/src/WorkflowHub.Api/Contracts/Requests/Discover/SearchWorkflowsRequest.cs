namespace WorkflowHub.Api.Contracts.Requests.Discover;

public sealed class SearchWorkflowsRequest
{
    public string? Query { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string? SortBy { get; set; }
    public Dictionary<string, string>? Filters { get; set; }
}
