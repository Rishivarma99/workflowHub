using Microsoft.EntityFrameworkCore;
using WorkflowHub.Data.Abstractions.Identity;
using WorkflowHub.Data.Entities;

namespace WorkflowHub.Data.Persistence;

public sealed class AppDbContext : DbContext
{
    private readonly ICurrentUserContext _currentUserContext;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : this(options, NullCurrentUserContext.Instance)
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserContext currentUserContext)
        : base(options)
    {
        _currentUserContext = currentUserContext;
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<UserIdentity> UserIdentities => Set<UserIdentity>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Workflow> Workflows => Set<Workflow>();

    public DbSet<WorkflowComponent> WorkflowComponents => Set<WorkflowComponent>();

    public DbSet<WorkflowStar> WorkflowStars => Set<WorkflowStar>();

    public DbSet<WorkflowComponentStar> WorkflowComponentStars => Set<WorkflowComponentStar>();

    public DbSet<ArticleCategory> ArticleCategories => Set<ArticleCategory>();

    public DbSet<Article> Articles => Set<Article>();

    public override int SaveChanges()
    {
        AuditFieldApplier.Apply(ChangeTracker, _currentUserContext.UserId);
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AuditFieldApplier.Apply(ChangeTracker, _currentUserContext.UserId);
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AuditFieldApplier.Apply(ChangeTracker, _currentUserContext.UserId);
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        AuditFieldApplier.Apply(ChangeTracker, _currentUserContext.UserId);
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
