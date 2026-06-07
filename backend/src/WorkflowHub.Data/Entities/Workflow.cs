using WorkflowHub.Data.Entities.Abstractions;

namespace WorkflowHub.Data.Entities;

public sealed class Workflow : ITrackable, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Tags { get; set; } = [];
    public string RepositoryUrl { get; set; } = string.Empty;
    public string CommitSha { get; set; } = string.Empty;
    public string WorkflowCode { get; set; } = string.Empty;
    public string[] BuiltForAgents { get; set; } = [];
    public string SourceIde { get; set; } = string.Empty;
    public string Complexity { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public int StarCount { get; set; }
    public int DownloadCount { get; set; }
    public string[] ComponentTypes { get; set; } = [];
    public List<WorkflowDependencyEntry> Dependencies { get; set; } = [];
    public string SearchText { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public List<WorkflowComponent> Components { get; set; } = [];

    public void SoftDeleteWithComponents(Guid? userId)
    {
        IsDeleted = true;
        UpdatedAtUtc = DateTime.UtcNow;
        UpdatedByUserId = userId;

        foreach (var component in Components)
        {
            component.SoftDelete(userId);
        }
    }
}
