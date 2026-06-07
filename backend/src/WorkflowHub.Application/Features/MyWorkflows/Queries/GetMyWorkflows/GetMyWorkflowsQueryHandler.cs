using Microsoft.EntityFrameworkCore;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.MyWorkflows.ReadModels;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Features.MyWorkflows.Queries.GetMyWorkflows;

public sealed class GetMyWorkflowsQueryHandler(AppDbContext dbContext)
    : IQueryHandler<GetMyWorkflowsQuery, MyWorkflowsSummaryDto>
{
    public async Task<MyWorkflowsSummaryDto> Handle(
        GetMyWorkflowsQuery query,
        CancellationToken cancellationToken)
    {
        var rows = await dbContext.Workflows
            .AsNoTracking()
            .Where(w => w.OwnerId == query.OwnerId)
            .OrderByDescending(w => w.UpdatedAtUtc ?? w.CreatedAtUtc)
            .Select(w => new
            {
                w.Id,
                w.Name,
                w.Description,
                w.SourceIde,
                w.StarCount,
                w.DownloadCount,
                ComponentCount = w.Components.Count,
                UpdatedAtUtc = w.UpdatedAtUtc ?? w.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(w => new MyWorkflowListItemDto(
                w.Id,
                w.Name,
                w.Description,
                w.SourceIde,
                w.StarCount,
                w.DownloadCount,
                w.ComponentCount,
                w.UpdatedAtUtc))
            .ToList();

        return new MyWorkflowsSummaryDto(
            items,
            items.Count,
            items.Sum(i => i.DownloadCount),
            items.Sum(i => i.ComponentCount));
    }
}
