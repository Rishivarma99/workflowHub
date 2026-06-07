using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WorkflowHub.Data.Entities;
using WorkflowHub.Data.Entities.Abstractions;

namespace WorkflowHub.Data.Persistence;

internal static class AuditFieldApplier
{
    private static readonly HashSet<string> WorkflowCounterProperties =
        new(StringComparer.Ordinal) { nameof(Workflow.StarCount), nameof(Workflow.DownloadCount) };

    private static readonly HashSet<string> ComponentCounterProperties =
        new(StringComparer.Ordinal) { nameof(WorkflowComponent.StarCount) };

    public static void Apply(ChangeTracker changeTracker, Guid? userId)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in changeTracker.Entries())
        {
            if (entry.Entity is ISoftDeletable softDeletable && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                softDeletable.IsDeleted = true;
            }

            if (entry.Entity is not ITrackable trackable)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                trackable.CreatedAtUtc = now;
                trackable.CreatedByUserId ??= userId;
                continue;
            }

            if (entry.State == EntityState.Modified && !ShouldSkipAuditUpdate(entry))
            {
                trackable.UpdatedAtUtc = now;
                trackable.UpdatedByUserId = userId;
            }
        }
    }

    private static bool ShouldSkipAuditUpdate(EntityEntry entry)
    {
        var modifiedNames = entry.Properties
            .Where(p => p.IsModified)
            .Select(p => p.Metadata.Name)
            .ToList();

        if (modifiedNames.Count == 0)
        {
            return true;
        }

        return entry.Entity switch
        {
            Workflow => modifiedNames.All(WorkflowCounterProperties.Contains),
            WorkflowComponent => modifiedNames.All(ComponentCounterProperties.Contains),
            _ => false
        };
    }
}
