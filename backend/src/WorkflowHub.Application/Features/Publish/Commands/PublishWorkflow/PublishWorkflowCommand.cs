using WorkflowHub.Application.CQRS.Abstractions;

namespace WorkflowHub.Application.Features.Publish.Commands.PublishWorkflow;

public sealed record PublishWorkflowCommand(
    Guid OwnerId,
    string RepositoryUrl,
    IReadOnlyList<string> BuiltForAgents,
    string WorkflowName,
    string Description,
    IReadOnlyList<string> Tags,
    IReadOnlyList<PublishDependencyDto> Dependencies,
    string Complexity,
    string TargetAudience,
    PublishAnalysisDto Analysis) : ICommand<PublishWorkflowResultDto>, ITransactionalCommand;
