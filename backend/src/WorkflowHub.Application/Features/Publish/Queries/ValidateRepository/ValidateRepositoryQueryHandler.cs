using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Publish.Services;

namespace WorkflowHub.Application.Features.Publish.Queries.ValidateRepository;

public sealed class ValidateRepositoryQueryHandler(IGitHubRepositoryClient gitHubClient)
    : IQueryHandler<ValidateRepositoryQuery, RepoValidationDto>
{
    public async Task<RepoValidationDto> Handle(
        ValidateRepositoryQuery query,
        CancellationToken cancellationToken)
    {
        var status = await gitHubClient.InspectPublicRepositoryAsync(query.RepositoryUrl, cancellationToken);
        return new RepoValidationDto(status.Exists, status.IsPublic, status.Accessible);
    }
}
