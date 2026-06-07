using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowHub.Data.Entities;

namespace WorkflowHub.Data.Persistence.Configurations;

public sealed class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.ToTable("workflows");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.HasIndex(w => w.Name)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.Property(w => w.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(w => w.Tags)
            .HasColumnType("text[]");

        builder.Property(w => w.RepositoryUrl)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(w => w.CommitSha)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(w => w.WorkflowCode)
            .HasMaxLength(16)
            .IsRequired();

        builder.HasIndex(w => w.WorkflowCode)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.Property(w => w.BuiltForAgents)
            .HasColumnType("text[]");

        builder.Property(w => w.SourceIde)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(w => w.Complexity)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(w => w.TargetAudience)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(w => w.ComponentTypes)
            .HasColumnType("text[]");

        builder.Property(w => w.Dependencies)
            .HasColumnType("jsonb")
            .HasConversion(new JsonListValueConverter<WorkflowDependencyEntry>());

        builder.Property(w => w.SearchText)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(w => w.IsDeleted)
            .IsRequired();

        builder.Property(w => w.CreatedAtUtc)
            .IsRequired();

        builder.Property(w => w.CreatedByUserId);

        builder.Property(w => w.UpdatedAtUtc);

        builder.Property(w => w.UpdatedByUserId);

        builder.HasQueryFilter(w => !w.IsDeleted);

        builder.HasOne(w => w.Owner)
            .WithMany()
            .HasForeignKey(w => w.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(w => w.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(w => w.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.Components)
            .WithOne(c => c.Workflow)
            .HasForeignKey(c => c.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
