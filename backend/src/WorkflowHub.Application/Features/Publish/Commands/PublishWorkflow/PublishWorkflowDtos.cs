namespace WorkflowHub.Application.Features.Publish.Commands.PublishWorkflow;

public sealed record PublishWorkflowResultDto(Guid Id, string WorkflowName, string WorkflowCode);

public sealed record PublishAnalysisCapabilityDto(string Name, string Description);

public sealed record PublishAnalysisComponentDto(
    string Path,
    string ComponentType,
    string Title,
    string Summary,
    IReadOnlyList<PublishAnalysisCapabilityDto> Capabilities,
    IReadOnlyList<string> Keywords,
    IReadOnlyList<string> SearchPhrases,
    IReadOnlyList<string> Technologies,
    IReadOnlyList<string> Dependencies);

public sealed record PublishAnalysisDto(
    string WorkflowName,
    string Description,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> SuggestedDependencies,
    IReadOnlyList<PublishAnalysisComponentDto> Components);

public sealed record PublishDependencyDto(
    string Kind,
    string Name,
    string Requirement,
    string Note);
