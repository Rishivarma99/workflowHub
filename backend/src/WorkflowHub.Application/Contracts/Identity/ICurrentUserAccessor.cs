using System.Security.Claims;

namespace WorkflowHub.Application.Contracts.Identity;

public interface ICurrentUserAccessor
{
    Guid? GetUserId(ClaimsPrincipal principal);
}
