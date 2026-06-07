namespace WorkflowHub.Application.Features.Publish.Queries.ValidateRepository;

public sealed record RepoValidationDto(bool Exists, bool IsPublic, bool Accessible);
