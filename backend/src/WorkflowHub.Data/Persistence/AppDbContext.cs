using Microsoft.EntityFrameworkCore;

namespace WorkflowHub.Data.Persistence;

/// <summary>
/// EF Core database context for Workflow Hub.
/// Stub only: no entities mapped yet. Entity configurations are applied
/// from <c>Persistence/Configurations</c> as features are added.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Entity configurations will be applied here as features land, e.g.:
        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
