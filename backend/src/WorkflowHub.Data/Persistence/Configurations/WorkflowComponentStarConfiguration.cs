using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowHub.Data.Entities;

namespace WorkflowHub.Data.Persistence.Configurations;

public sealed class WorkflowComponentStarConfiguration : IEntityTypeConfiguration<WorkflowComponentStar>
{
    public void Configure(EntityTypeBuilder<WorkflowComponentStar> builder)
    {
        builder.ToTable("workflow_component_stars");

        builder.HasKey(s => s.Id);

        builder.HasIndex(s => new { s.UserId, s.WorkflowComponentId }).IsUnique();

        builder.Property(s => s.CreatedAtUtc).IsRequired();

        builder.HasOne<WorkflowComponent>()
            .WithMany()
            .HasForeignKey(s => s.WorkflowComponentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
