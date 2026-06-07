namespace WorkflowHub.Application.Features.Install.Services;

public static class WorkflowCodeGenerator
{
    private const string Prefix = "WF-";
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Generate()
    {
        Span<char> suffix = stackalloc char[5];
        for (var i = 0; i < suffix.Length; i++)
        {
            suffix[i] = Alphabet[Random.Shared.Next(Alphabet.Length)];
        }

        return Prefix + new string(suffix);
    }
}
