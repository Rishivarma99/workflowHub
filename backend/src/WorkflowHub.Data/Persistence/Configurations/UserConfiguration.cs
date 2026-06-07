using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowHub.Data.Entities;

namespace WorkflowHub.Data.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.Property(u => u.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.DisplayName)
            .HasMaxLength(50);

        builder.Property(u => u.Username)
            .HasMaxLength(30);

        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasFilter("\"Username\" IS NOT NULL AND \"IsDeleted\" = false");

        builder.Property(u => u.Role)
            .HasMaxLength(80);

        builder.Property(u => u.Team)
            .HasMaxLength(80);

        builder.Property(u => u.Bio)
            .HasMaxLength(160);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(2048);

        builder.Property(u => u.SecurityStamp)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(u => u.IsDeleted)
            .IsRequired();

        builder.Property(u => u.CreatedAtUtc)
            .IsRequired();

        builder.Property(u => u.CreatedByUserId);

        builder.Property(u => u.UpdatedAtUtc);

        builder.Property(u => u.UpdatedByUserId);

        builder.HasQueryFilter(u => !u.IsDeleted);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(u => u.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(u => u.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
