namespace WorkflowHub.Application.Features.Learn.Services;

internal static class LearnSearchHelper
{
    private static readonly HashSet<string> StopWords =
    [
        "a", "an", "the", "to", "of", "in", "on", "and", "or", "for", "with", "by", "from", "into", "is", "it", "my", "me"
    ];

    internal static IReadOnlyList<string> Tokenize(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        return query
            .Trim()
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => t.Length > 1 && !StopWords.Contains(t))
            .Distinct()
            .ToList();
    }

    internal static string BuildSearchText(string title, string summary, string content, string categoryName) =>
        string.Join(' ',
            title,
            summary,
            categoryName,
            StripMarkdown(content));

    private static string StripMarkdown(string markdown) =>
        markdown
            .Replace('#', ' ')
            .Replace('*', ' ')
            .Replace('`', ' ')
            .Replace('[', ' ')
            .Replace(']', ' ')
            .Replace('(', ' ')
            .Replace(')', ' ');
}
