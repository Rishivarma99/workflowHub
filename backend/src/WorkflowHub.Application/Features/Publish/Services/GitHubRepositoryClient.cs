using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace WorkflowHub.Application.Features.Publish.Services;

public sealed class GitHubRepositoryClient(HttpClient httpClient, ILogger<GitHubRepositoryClient> logger)
    : IGitHubRepositoryClient
{
    public async Task<GitHubRepositoryStatus> InspectPublicRepositoryAsync(
        string repositoryUrl,
        CancellationToken cancellationToken = default)
    {
        var parsed = GitHubUrlParser.TryParse(repositoryUrl);
        if (parsed is null)
        {
            return new GitHubRepositoryStatus(false, false, false, null, null);
        }

        try
        {
            using var repoResponse = await httpClient.GetAsync(
                $"repos/{parsed.Owner}/{parsed.Repo}",
                cancellationToken);

            if (repoResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return new GitHubRepositoryStatus(false, false, false, null, parsed);
            }

            if (!repoResponse.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "GitHub repo lookup failed for {Owner}/{Repo}: {Status}",
                    parsed.Owner,
                    parsed.Repo,
                    repoResponse.StatusCode);
                return new GitHubRepositoryStatus(true, false, false, null, parsed);
            }

            var repo = await repoResponse.Content.ReadFromJsonAsync<GitHubRepoResponse>(
                cancellationToken: cancellationToken);

            if (repo is null)
            {
                return new GitHubRepositoryStatus(true, false, false, null, parsed);
            }

            var isPublic = !repo.Private;
            if (!isPublic)
            {
                return new GitHubRepositoryStatus(true, false, false, null, parsed);
            }

            var branch = string.IsNullOrWhiteSpace(repo.DefaultBranch) ? "main" : repo.DefaultBranch;
            using var commitResponse = await httpClient.GetAsync(
                $"repos/{parsed.Owner}/{parsed.Repo}/commits/{branch}",
                cancellationToken);

            if (!commitResponse.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "GitHub commit lookup failed for {Owner}/{Repo}@{Branch}: {Status}",
                    parsed.Owner,
                    parsed.Repo,
                    branch,
                    commitResponse.StatusCode);
                return new GitHubRepositoryStatus(true, true, false, null, parsed);
            }

            var commit = await commitResponse.Content.ReadFromJsonAsync<GitHubCommitResponse>(
                cancellationToken: cancellationToken);

            var sha = commit?.Sha;
            if (string.IsNullOrWhiteSpace(sha))
            {
                return new GitHubRepositoryStatus(true, true, false, null, parsed);
            }

            return new GitHubRepositoryStatus(true, true, true, sha, parsed);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "GitHub API request failed for {Url}", repositoryUrl);
            return new GitHubRepositoryStatus(false, false, false, null, parsed);
        }
    }

    private sealed class GitHubRepoResponse
    {
        [JsonPropertyName("private")]
        public bool Private { get; set; }

        [JsonPropertyName("default_branch")]
        public string? DefaultBranch { get; set; }
    }

    private sealed class GitHubCommitResponse
    {
        [JsonPropertyName("sha")]
        public string? Sha { get; set; }
    }
}
