namespace WorkflowHub.Common.Errors;

public static class Errors
{
    public static class Auth
    {
        public static readonly ErrorDefinition UnknownProvider = new()
        {
            Code = "UNKNOWN_PROVIDER",
            MessageTemplate = "Unknown auth provider: {Provider}",
            StatusCode = 400
        };

        public static readonly ErrorDefinition ExternalAuthFailed = new()
        {
            Code = "EXTERNAL_AUTH_FAILED",
            MessageTemplate = "External authentication failed.",
            StatusCode = 400
        };

        public static readonly ErrorDefinition InvalidRefreshToken = new()
        {
            Code = "INVALID_REFRESH_TOKEN",
            MessageTemplate = "Refresh token is invalid or expired.",
            StatusCode = 401
        };

        public static readonly ErrorDefinition UserNotFound = new()
        {
            Code = "USER_NOT_FOUND",
            MessageTemplate = "User not found.",
            StatusCode = 404
        };

        public static readonly ErrorDefinition UsernameTaken = new()
        {
            Code = "USERNAME_TAKEN",
            MessageTemplate = "That username is already taken.",
            StatusCode = 409
        };
    }

    public static class Workflow
    {
        public static readonly ErrorDefinition NameTaken = new()
        {
            Code = "WORKFLOW_NAME_TAKEN",
            MessageTemplate = "Workflow name already exists.",
            StatusCode = 409
        };

        public static readonly ErrorDefinition RepositoryUnavailable = new()
        {
            Code = "REPOSITORY_UNAVAILABLE",
            MessageTemplate = "The repository is missing, private, or inaccessible.",
            StatusCode = 400
        };

        public static readonly ErrorDefinition InvalidAnalysis = new()
        {
            Code = "INVALID_ANALYSIS",
            MessageTemplate = "Analysis JSON failed validation.",
            StatusCode = 400
        };

        public static readonly ErrorDefinition NotFound = new()
        {
            Code = "WORKFLOW_NOT_FOUND",
            MessageTemplate = "Workflow not found.",
            StatusCode = 404
        };

        public static readonly ErrorDefinition Forbidden = new()
        {
            Code = "WORKFLOW_FORBIDDEN",
            MessageTemplate = "You do not have permission to modify this workflow.",
            StatusCode = 403
        };
    }

    public static class AgentAsset
    {
        public static readonly ErrorDefinition NotFound = new()
        {
            Code = "AGENT_ASSET_NOT_FOUND",
            MessageTemplate = "Agent asset not found.",
            StatusCode = 404
        };
    }

    public static class Search
    {
        public static readonly ErrorDefinition QueryRequired = new()
        {
            Code = "SEARCH_QUERY_REQUIRED",
            MessageTemplate = "A non-empty search query is required.",
            StatusCode = 400
        };
    }

    public static class Internal
    {
        public static readonly ErrorDefinition InternalServerError = new()
        {
            Code = "INTERNAL_SERVER_ERROR",
            MessageTemplate = "Something went wrong",
            StatusCode = 500
        };
    }
}
