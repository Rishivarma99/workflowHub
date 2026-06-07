using WorkflowHub.Application.Features.Publish.Commands.PublishWorkflow;
using WorkflowHub.Data.Entities;

namespace WorkflowHub.Application.Features.Publish.Services;

public static class PublishWorkflowMapper
{
    private static readonly HashSet<string> AllowedComponentTypes =
    [
        "rule", "command", "hook", "skill", "subagent"
    ];

    public static IReadOnlyList<string> ValidateAnalysis(PublishAnalysisDto analysis)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(analysis.WorkflowName))
        {
            errors.Add("workflowName: required");
        }

        if (string.IsNullOrWhiteSpace(analysis.Description))
        {
            errors.Add("description: required");
        }

        if (analysis.Tags.Count == 0)
        {
            errors.Add("tags: at least one tag required");
        }

        if (analysis.Components.Count == 0)
        {
            errors.Add("components: at least one component required");
        }

        for (var i = 0; i < analysis.Components.Count; i++)
        {
            ValidateComponent(analysis.Components[i], i, errors);
        }

        return errors;
    }

    public static Workflow MapToEntity(
        Guid ownerId,
        string repositoryUrl,
        string commitSha,
        IReadOnlyList<string> builtForAgents,
        string workflowCode,
        string workflowName,
        string description,
        IReadOnlyList<string> tags,
        IReadOnlyList<PublishDependencyDto> dependencies,
        string complexity,
        string targetAudience,
        PublishAnalysisDto analysis,
        GitHubRepositoryRef repositoryRef)
    {
        var now = DateTime.UtcNow;
        var workflowId = Guid.NewGuid();

        var components = analysis.Components.Select(c => new WorkflowComponent
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflowId,
            Path = c.Path.Trim(),
            GitHubUrl = GitHubUrlParser.BuildBlobUrl(repositoryRef, commitSha, c.Path),
            ComponentType = c.ComponentType.Trim().ToLowerInvariant(),
            Title = c.Title.Trim(),
            Summary = c.Summary.Trim(),
            CreatedByUserId = ownerId,
            Capabilities = c.Capabilities
                .Select(cap => new ComponentCapabilityEntry
                {
                    Name = cap.Name.Trim(),
                    Description = cap.Description.Trim()
                })
                .ToList(),
            Keywords = c.Keywords.Select(k => k.Trim()).Where(k => k.Length > 0).ToArray(),
            SearchPhrases = c.SearchPhrases.Select(p => p.Trim()).Where(p => p.Length > 0).ToArray(),
            Technologies = c.Technologies.Select(t => t.Trim()).Where(t => t.Length > 0).ToArray(),
            Dependencies = c.Dependencies.Select(d => d.Trim()).Where(d => d.Length > 0).ToArray()
        }).ToList();

        foreach (var component in components)
        {
            component.SearchText = BuildComponentSearchText(component);
        }

        var componentTypes = components
            .Select(c => c.ComponentType)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(t => t, StringComparer.Ordinal)
            .ToArray();

        var normalizedAgents = builtForAgents
            .Select(a => a.Trim().ToLowerInvariant())
            .Where(a => a.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(a => a, StringComparer.Ordinal)
            .ToArray();

        var workflow = new Workflow
        {
            Id = workflowId,
            OwnerId = ownerId,
            Name = workflowName.Trim(),
            Description = description.Trim(),
            Tags = tags.Select(t => t.Trim().ToLowerInvariant()).Where(t => t.Length > 0).ToArray(),
            RepositoryUrl = repositoryRef.NormalizedUrl,
            CommitSha = commitSha,
            WorkflowCode = workflowCode.Trim().ToUpperInvariant(),
            BuiltForAgents = normalizedAgents,
            SourceIde = normalizedAgents.Length > 0 ? normalizedAgents[0] : string.Empty,
            Complexity = complexity.Trim().ToLowerInvariant(),
            TargetAudience = targetAudience.Trim().ToLowerInvariant(),
            StarCount = 0,
            DownloadCount = 0,
            ComponentTypes = componentTypes,
            Dependencies = dependencies
                .Select(d => new WorkflowDependencyEntry
                {
                    Kind = d.Kind.Trim().ToLowerInvariant(),
                    Name = d.Name.Trim(),
                    Requirement = d.Requirement.Trim().ToLowerInvariant(),
                    Note = d.Note.Trim()
                })
                .ToList(),
            CreatedAtUtc = now,
            CreatedByUserId = ownerId,
            Components = components
        };

        workflow.SearchText = BuildSearchText(workflow, components);
        return workflow;
    }

    public static string BuildNameSuggestion(string workflowName, GitHubRepositoryRef repositoryRef)
    {
        var trimmed = workflowName.Trim();
        return $"{trimmed} ({repositoryRef.Repo})";
    }

    private static void ValidateComponent(PublishAnalysisComponentDto component, int index, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(component.Path))
        {
            errors.Add($"components[{index}].path: required");
        }

        if (!AllowedComponentTypes.Contains(component.ComponentType.Trim().ToLowerInvariant()))
        {
            errors.Add($"components[{index}].componentType: invalid");
        }

        if (string.IsNullOrWhiteSpace(component.Title))
        {
            errors.Add($"components[{index}].title: required");
        }

        if (string.IsNullOrWhiteSpace(component.Summary))
        {
            errors.Add($"components[{index}].summary: required");
        }

        if (component.Capabilities.Count == 0)
        {
            errors.Add($"components[{index}].capabilities: at least one required");
        }

        if (component.Keywords.Count == 0)
        {
            errors.Add($"components[{index}].keywords: at least one required");
        }

        if (component.SearchPhrases.Count == 0)
        {
            errors.Add($"components[{index}].searchPhrases: at least one required");
        }
    }

    private static string BuildComponentSearchText(WorkflowComponent component)
    {
        var parts = new List<string>
        {
            component.Title,
            component.Summary,
            component.Path,
            component.ComponentType,
            string.Join(' ', component.Keywords),
            string.Join(' ', component.SearchPhrases),
            string.Join(' ', component.Technologies)
        };

        foreach (var capability in component.Capabilities)
        {
            parts.Add(capability.Name);
            parts.Add(capability.Description);
        }

        return string.Join('\n', parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    private static string BuildSearchText(Workflow workflow, IReadOnlyList<WorkflowComponent> components)
    {
        var parts = new List<string>
        {
            workflow.Name,
            workflow.Description,
            string.Join(' ', workflow.Tags)
        };

        foreach (var component in components)
        {
            parts.Add(component.Title);
            parts.Add(component.Summary);
            parts.Add(string.Join(' ', component.Keywords));
            parts.Add(string.Join(' ', component.SearchPhrases));
            parts.Add(string.Join(' ', component.Technologies));
            parts.Add(string.Join(' ', component.Capabilities.Select(c => $"{c.Name} {c.Description}")));
        }

        return string.Join('\n', parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }
}
