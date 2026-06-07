namespace WorkflowHub.Data.Abstractions.Identity;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
}
