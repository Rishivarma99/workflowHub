using WorkflowHub.Data.Entities.Abstractions;

namespace WorkflowHub.Data.Entities;

public sealed class WorkflowComponent : ITrackable, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public Workflow Workflow { get; set; } = null!;
    public string Path { get; set; } = string.Empty;
    public string GitHubUrl { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string SearchText { get; set; } = string.Empty;
    public List<ComponentCapabilityEntry> Capabilities { get; set; } = [];
    public string[] Keywords { get; set; } = [];
    public string[] SearchPhrases { get; set; } = [];
    public string[] Technologies { get; set; } = [];
    public string[] Dependencies { get; set; } = [];
    public int StarCount { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? UpdatedByUserId { get; set; }

    public void SoftDelete(Guid? userId)
    {
        IsDeleted = true;
        UpdatedAtUtc = DateTime.UtcNow;
        UpdatedByUserId = userId;
    }
}
