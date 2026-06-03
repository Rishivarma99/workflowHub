using WorkflowHub.Common.Errors;

namespace WorkflowHub.Common.Exceptions;

public sealed class AppException : Exception
{
    public ErrorDefinition Error { get; }
    public Dictionary<string, object>? Params { get; }

    public AppException(ErrorDefinition error, Dictionary<string, object>? @params = null)
        : base(error.MessageTemplate)
    {
        Error = error;
        Params = @params;
    }
}
