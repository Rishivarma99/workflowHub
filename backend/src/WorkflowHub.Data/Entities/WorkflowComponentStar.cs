namespace WorkflowHub.Data.Entities;

public sealed class WorkflowComponentStar
{
    public Guid Id { get; set; }
    public Guid WorkflowComponentId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
