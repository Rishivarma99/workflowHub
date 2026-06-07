using WorkflowHub.Application.CQRS.Abstractions;

namespace WorkflowHub.Application.Features.Publish.Queries.ValidateRepository;

public sealed record ValidateRepositoryQuery(string RepositoryUrl) : IQuery<RepoValidationDto>;
