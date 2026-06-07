using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowHub.Data.Entities;

namespace WorkflowHub.Data.Persistence.Configurations;

public sealed class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("articles");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Slug)
            .HasMaxLength(160)
            .IsRequired();

        builder.HasIndex(a => a.Slug)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.Property(a => a.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Summary)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.Content)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(a => a.ContentType)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(a => a.SearchText)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.IsDeleted)
            .IsRequired();

        builder.Property(a => a.CreatedAtUtc)
            .IsRequired();

        builder.HasQueryFilter(a => !a.IsDeleted);

        builder.HasOne(a => a.Category)
            .WithMany(c => c.Articles)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Author)
            .WithMany()
            .HasForeignKey(a => a.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
