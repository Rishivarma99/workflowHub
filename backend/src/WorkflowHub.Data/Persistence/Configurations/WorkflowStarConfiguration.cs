using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowHub.Data.Entities;

namespace WorkflowHub.Data.Persistence.Configurations;

public sealed class WorkflowStarConfiguration : IEntityTypeConfiguration<WorkflowStar>
{
    public void Configure(EntityTypeBuilder<WorkflowStar> builder)
    {
        builder.ToTable("workflow_stars");

        builder.HasKey(s => s.Id);

        builder.HasIndex(s => new { s.UserId, s.WorkflowId }).IsUnique();

        builder.Property(s => s.CreatedAtUtc).IsRequired();

        builder.HasOne<Workflow>()
            .WithMany()
            .HasForeignKey(s => s.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
