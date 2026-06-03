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
