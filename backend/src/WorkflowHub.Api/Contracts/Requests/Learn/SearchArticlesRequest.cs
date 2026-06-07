namespace WorkflowHub.Api.Contracts.Requests.Learn;

public sealed class SearchArticlesRequest
{
    public string? Query { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Dictionary<string, string>? Filters { get; set; }
}
