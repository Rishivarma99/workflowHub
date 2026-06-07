namespace WorkflowHub.Data.Abstractions.Identity;

public sealed class NullCurrentUserContext : ICurrentUserContext
{
    public static readonly NullCurrentUserContext Instance = new();

    public Guid? UserId => null;
}
