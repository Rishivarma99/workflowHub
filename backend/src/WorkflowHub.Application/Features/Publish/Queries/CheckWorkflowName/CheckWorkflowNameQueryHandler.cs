using Microsoft.EntityFrameworkCore;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Data.Persistence;

namespace WorkflowHub.Application.Features.Publish.Queries.CheckWorkflowName;

public sealed class CheckWorkflowNameQueryHandler(AppDbContext dbContext)
    : IQueryHandler<CheckWorkflowNameQuery, NameAvailabilityDto>
{
    public async Task<NameAvailabilityDto> Handle(
        CheckWorkflowNameQuery query,
        CancellationToken cancellationToken)
    {
        var normalized = query.Name.Trim().ToLowerInvariant();
        var exists = await dbContext.Workflows.AnyAsync(
            w => w.Name.ToLower() == normalized,
            cancellationToken);

        if (!exists)
        {
            return new NameAvailabilityDto(true, null);
        }

        var suggestion = $"{query.Name.Trim()} 2";
        return new NameAvailabilityDto(false, suggestion);
    }
}
