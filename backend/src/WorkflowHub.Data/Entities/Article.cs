using WorkflowHub.Data.Entities.Abstractions;

namespace WorkflowHub.Data.Entities;

public sealed class Article : ITrackable, ISoftDeletable
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public ArticleCategory Category { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public ArticleStatus Status { get; set; }
    public ArticleContentType ContentType { get; set; }
    public bool IsPinned { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public string SearchText { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? UpdatedByUserId { get; set; }
}
