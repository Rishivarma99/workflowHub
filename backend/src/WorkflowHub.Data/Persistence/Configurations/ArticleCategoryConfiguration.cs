using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowHub.Data.Entities;

namespace WorkflowHub.Data.Persistence.Configurations;

public sealed class ArticleCategoryConfiguration : IEntityTypeConfiguration<ArticleCategory>
{
    public void Configure(EntityTypeBuilder<ArticleCategory> builder)
    {
        builder.ToTable("article_categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(c => c.Slug)
            .HasMaxLength(120)
            .IsRequired();

        builder.HasIndex(c => c.Slug)
            .IsUnique();

        builder.Property(c => c.Description)
            .HasMaxLength(500)
            .IsRequired();
    }
}
