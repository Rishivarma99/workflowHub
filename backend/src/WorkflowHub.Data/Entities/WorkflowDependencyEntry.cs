namespace WorkflowHub.Data.Entities;

public sealed class WorkflowDependencyEntry
{
    public string Kind { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Requirement { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}
