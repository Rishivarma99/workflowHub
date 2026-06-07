namespace WorkflowHub.Application.Features.Discover.ReadModels;

public sealed record WorkflowAuthorDto(
    string Name,
    string? DisplayName,
    string? AvatarUrl);

public sealed record MatchedComponentDto(
    Guid ComponentId,
    string ComponentType,
    string Path,
    string Title,
    string CapabilityName);

public sealed record WorkflowCardDto(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyList<string> Tags,
    string WorkflowCode,
    IReadOnlyList<string> BuiltForAgents,
    string SourceIde,
    int StarCount,
    int DownloadCount,
    bool IsStarred,
    IReadOnlyList<string> ComponentTypes,
    IReadOnlyDictionary<string, int> ComponentCounts,
    WorkflowAuthorDto Author,
    MatchedComponentDto? MatchedComponent);

public sealed record WorkflowCapabilityDto(string Name, string Description);

public sealed record WorkflowComponentDetailDto(
    Guid Id,
    string Path,
    string GitHubUrl,
    string ComponentType,
    string Title,
    string Summary,
    int StarCount,
    bool IsStarred,
    IReadOnlyList<WorkflowCapabilityDto> Capabilities,
    IReadOnlyList<string> Keywords,
    IReadOnlyList<string> SearchPhrases,
    IReadOnlyList<string> Technologies,
    IReadOnlyList<string> Dependencies);

public sealed record WorkflowDependencyDetailDto(
    string Kind,
    string Name,
    string Requirement,
    string? Note);

public sealed record WorkflowDetailDto(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyList<string> Tags,
    string RepositoryUrl,
    string CommitSha,
    string WorkflowCode,
    IReadOnlyList<string> BuiltForAgents,
    string SourceIde,
    string Complexity,
    string TargetAudience,
    int StarCount,
    int DownloadCount,
    bool IsStarred,
    DateTime UpdatedAtUtc,
    WorkflowAuthorDto Author,
    IReadOnlyList<WorkflowDependencyDetailDto> Dependencies,
    IReadOnlyList<WorkflowComponentDetailDto> Components);
