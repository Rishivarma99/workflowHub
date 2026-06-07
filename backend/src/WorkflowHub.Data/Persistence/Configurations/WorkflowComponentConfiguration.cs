using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowHub.Data.Entities;

namespace WorkflowHub.Data.Persistence.Configurations;

public sealed class WorkflowComponentConfiguration : IEntityTypeConfiguration<WorkflowComponent>
{
    public void Configure(EntityTypeBuilder<WorkflowComponent> builder)
    {
        builder.ToTable("workflow_components");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Path)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(c => c.GitHubUrl)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(c => c.ComponentType)
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(c => c.ComponentType);

        builder.Property(c => c.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Summary)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(c => c.SearchText)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(c => c.Capabilities)
            .HasColumnType("jsonb")
            .HasConversion(new JsonListValueConverter<ComponentCapabilityEntry>());

        builder.Property(c => c.Keywords)
            .HasColumnType("text[]");

        builder.Property(c => c.SearchPhrases)
            .HasColumnType("text[]");

        builder.Property(c => c.Technologies)
            .HasColumnType("text[]");

        builder.Property(c => c.Dependencies)
            .HasColumnType("text[]");

        builder.Property(c => c.IsDeleted)
            .IsRequired();

        builder.Property(c => c.CreatedAtUtc)
            .IsRequired();

        builder.Property(c => c.CreatedByUserId);

        builder.Property(c => c.UpdatedAtUtc);

        builder.Property(c => c.UpdatedByUserId);

        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
