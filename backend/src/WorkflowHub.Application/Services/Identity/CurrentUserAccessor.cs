using System.Security.Claims;
using WorkflowHub.Application.Contracts.Identity;

namespace WorkflowHub.Application.Services.Identity;

public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    public Guid? GetUserId(ClaimsPrincipal principal)
    {
        var sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
