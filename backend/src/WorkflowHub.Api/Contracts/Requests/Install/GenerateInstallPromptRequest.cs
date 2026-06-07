namespace WorkflowHub.Api.Contracts.Requests.Install;

public sealed class GenerateInstallPromptRequest
{
    public string TargetAgent { get; set; } = "same";
    public string InstallLevel { get; set; } = "project";
}
