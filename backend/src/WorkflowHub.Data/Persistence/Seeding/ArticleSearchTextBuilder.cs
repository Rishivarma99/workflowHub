namespace WorkflowHub.Data.Persistence.Seeding;

internal static class ArticleSearchTextBuilder
{
    internal static string Build(string title, string summary, string content, string categoryName) =>
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
