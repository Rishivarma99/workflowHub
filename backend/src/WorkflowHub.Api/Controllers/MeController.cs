using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowHub.Api.Contracts.Requests.Auth;
using WorkflowHub.Application.Auth.Models;
using WorkflowHub.Application.Contracts.Identity;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Auth.Commands.UpdateProfile;
using WorkflowHub.Application.Features.Auth.Queries.GetUserById;

namespace WorkflowHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
public sealed class MeController(
    ICurrentUserAccessor currentUser,
    IQueryDispatcher queryDispatcher,
    ICommandDispatcher commandDispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AuthUserDto>> Get(CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await queryDispatcher.Dispatch<AuthUserDto?>(
            new GetUserByIdQuery(userId.Value),
            cancellationToken);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }

    [HttpPatch]
    public async Task<ActionResult<AuthUserDto>> Patch(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var command = new UpdateProfileCommand(
            userId.Value,
            request.DisplayName,
            request.Username,
            request.Role,
            request.Team,
            request.Bio);
        var updated = await commandDispatcher.Dispatch<AuthUserDto>(command, cancellationToken);
        return Ok(updated);
    }
}
