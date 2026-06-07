using System.Text.RegularExpressions;

namespace WorkflowHub.Application.Features.Publish.Services;

public sealed partial record GitHubRepositoryRef(string Owner, string Repo, string NormalizedUrl);

public sealed record GitHubRepositoryStatus(
    bool Exists,
    bool IsPublic,
    bool Accessible,
    string? DefaultBranchSha,
    GitHubRepositoryRef? Repository);

public interface IGitHubRepositoryClient
{
    Task<GitHubRepositoryStatus> InspectPublicRepositoryAsync(
        string repositoryUrl,
        CancellationToken cancellationToken = default);
}

public static partial class GitHubUrlParser
{
    [GeneratedRegex(
        @"^https?://github\.com/(?<owner>[\w.-]+)/(?<repo>[\w.-]+?)(?:\.git)?/?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex RepoUrlPattern();

    public static GitHubRepositoryRef? TryParse(string repositoryUrl)
    {
        var trimmed = repositoryUrl.Trim();
        var match = RepoUrlPattern().Match(trimmed);
        if (!match.Success)
        {
            return null;
        }

        var owner = match.Groups["owner"].Value;
        var repo = match.Groups["repo"].Value;
        var normalized = $"https://github.com/{owner}/{repo}";
        return new GitHubRepositoryRef(owner, repo, normalized);
    }

    public static string BuildBlobUrl(GitHubRepositoryRef repository, string commitSha, string path)
    {
        var normalizedPath = path.TrimStart('/');
        return $"{repository.NormalizedUrl}/blob/{commitSha}/{normalizedPath}";
    }
}
