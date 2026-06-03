namespace WorkflowHub.Data.Entities;

public sealed class UserIdentity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ProviderSub { get; set; } = string.Empty;
    public string? ProviderEmail { get; set; }
    public string? ProviderAvatarUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
    public User User { get; set; } = default!;
}
