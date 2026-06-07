namespace WorkflowHub.Data.Entities.Abstractions;

public interface ITrackable
{
    DateTime CreatedAtUtc { get; set; }

    Guid? CreatedByUserId { get; set; }

    DateTime? UpdatedAtUtc { get; set; }

    Guid? UpdatedByUserId { get; set; }
}
