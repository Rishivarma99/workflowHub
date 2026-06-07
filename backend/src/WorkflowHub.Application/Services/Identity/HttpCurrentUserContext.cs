using Microsoft.AspNetCore.Http;
using WorkflowHub.Application.Contracts.Identity;
using WorkflowHub.Data.Abstractions.Identity;

namespace WorkflowHub.Application.Services.Identity;

public sealed class HttpCurrentUserContext(
    IHttpContextAccessor httpContextAccessor,
    ICurrentUserAccessor currentUserAccessor) : ICurrentUserContext
{
    public Guid? UserId
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User;
            return principal is null ? null : currentUserAccessor.GetUserId(principal);
        }
    }
}
