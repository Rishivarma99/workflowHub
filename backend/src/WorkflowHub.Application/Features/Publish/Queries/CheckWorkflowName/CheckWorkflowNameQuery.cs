using WorkflowHub.Application.CQRS.Abstractions;

namespace WorkflowHub.Application.Features.Publish.Queries.CheckWorkflowName;

public sealed record CheckWorkflowNameQuery(string Name) : IQuery<NameAvailabilityDto>;
