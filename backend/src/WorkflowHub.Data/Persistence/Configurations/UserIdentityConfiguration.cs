using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowHub.Data.Entities;

namespace WorkflowHub.Data.Persistence.Configurations;

public sealed class UserIdentityConfiguration : IEntityTypeConfiguration<UserIdentity>
{
    public void Configure(EntityTypeBuilder<UserIdentity> builder)
    {
        builder.ToTable("user_identities");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Provider)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(i => i.ProviderSub)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(i => i.ProviderEmail)
            .HasMaxLength(320);

        builder.Property(i => i.ProviderAvatarUrl)
            .HasMaxLength(2048);

        builder.HasIndex(i => new { i.Provider, i.ProviderSub })
            .IsUnique();

        builder.HasIndex(i => new { i.UserId, i.Provider })
            .IsUnique();

        builder.HasIndex(i => i.UserId);

        builder.Property(i => i.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(i => i.User)
            .WithMany(u => u.Identities)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
