namespace WorkflowHub.Data.Entities;

public sealed class WorkflowStar
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
