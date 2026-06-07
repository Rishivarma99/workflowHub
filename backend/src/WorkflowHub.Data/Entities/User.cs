using WorkflowHub.Data.Entities.Abstractions;

namespace WorkflowHub.Data.Entities;

public sealed class User : ITrackable, ISoftDeletable
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Username { get; set; }
    public string? Role { get; set; }
    public string? Team { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");
    public bool IsDeleted { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = [];
    public List<UserIdentity> Identities { get; set; } = [];
}
