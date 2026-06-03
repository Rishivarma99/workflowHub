namespace WorkflowHub.Common.Errors;

public static class ErrorFormatter
{
    public static string Format(string template, Dictionary<string, object>? data)
    {
        if (data is null)
        {
            return template;
        }

        foreach (var item in data)
        {
            template = template.Replace($"{{{item.Key}}}", item.Value?.ToString() ?? string.Empty);
        }

        return template;
    }
}
