namespace WorkflowHub.Application.Features.Publish.Queries.CheckWorkflowName;

public sealed record NameAvailabilityDto(bool Available, string? Suggestion);
