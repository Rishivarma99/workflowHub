namespace WorkflowHub.Data.Entities.Abstractions;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}
