using WorkflowHub.Data.Entities;

namespace WorkflowHub.Application.Features.Install.Services;

public static class InstallPromptService
{
    private static readonly Dictionary<string, string> AgentLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["cursor"] = "Cursor",
        ["claude"] = "Claude",
        ["copilot"] = "Copilot"
    };

    public static string BuildPrompt(
        Workflow workflow,
        string targetAgent,
        string installLevel)
    {
        var builtForDisplay = FormatAgents(workflow.BuiltForAgents);
        var resolvedTarget = ResolveTargetAgent(workflow.BuiltForAgents, targetAgent);
        var codeSuffix = workflow.WorkflowCode.StartsWith("WF-", StringComparison.OrdinalIgnoreCase)
            ? workflow.WorkflowCode[3..]
            : workflow.WorkflowCode;

        return InstallPromptTemplate.Template
            .Replace("{{WorkflowName}}", workflow.Name, StringComparison.Ordinal)
            .Replace("{{WorkflowCode}}", workflow.WorkflowCode, StringComparison.Ordinal)
            .Replace("{{RepositoryUrl}}", workflow.RepositoryUrl, StringComparison.Ordinal)
            .Replace("{{CommitSha}}", workflow.CommitSha, StringComparison.Ordinal)
            .Replace("{{BuiltForAgent}}", builtForDisplay, StringComparison.Ordinal)
            .Replace("{{TargetAgent}}", FormatAgent(resolvedTarget), StringComparison.Ordinal)
            .Replace("{{InstallLevel}}", FormatInstallLevel(installLevel), StringComparison.Ordinal)
            .Replace("{{WorkflowCodeSuffix}}", codeSuffix, StringComparison.Ordinal)
            .Replace("{{WorkflowCodeSuffixLower}}", codeSuffix.ToLowerInvariant(), StringComparison.Ordinal);
    }

    public static string ResolveTargetAgent(IReadOnlyList<string> builtForAgents, string targetAgent)
    {
        if (string.Equals(targetAgent, "same", StringComparison.OrdinalIgnoreCase))
        {
            return builtForAgents.Count > 0 ? builtForAgents[0] : "cursor";
        }

        return targetAgent.Trim().ToLowerInvariant();
    }

    private static string FormatAgents(IReadOnlyList<string> agents) =>
        string.Join(", ", agents.Select(FormatAgent));

    private static string FormatAgent(string agent) =>
        AgentLabels.TryGetValue(agent.Trim(), out var label) ? label : agent;

    private static string FormatInstallLevel(string installLevel) =>
        string.Equals(installLevel, "system", StringComparison.OrdinalIgnoreCase)
            ? "System Level"
            : "Project Level";
}
