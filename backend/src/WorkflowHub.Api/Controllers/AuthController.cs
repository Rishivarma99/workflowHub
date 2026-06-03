using Microsoft.AspNetCore.Mvc;
using WorkflowHub.Api.Contracts.Requests.Auth;
using WorkflowHub.Application.Auth;
using WorkflowHub.Application.Auth.Models;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Auth.Commands.ExternalLogin;
using WorkflowHub.Application.Features.Auth.Commands.Logout;
using WorkflowHub.Application.Features.Auth.Commands.RefreshSession;

namespace WorkflowHub.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ICommandDispatcher commandDispatcher) : ControllerBase
{
    [HttpPost("external")]
    public async Task<ActionResult<AuthResult>> ExternalLogin(
        [FromBody] ExternalLoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ExternalLoginCommand(
            request.Provider,
            request.IdToken,
            Request.Headers.UserAgent.ToString());

        var result = await commandDispatcher.Dispatch<AuthResult>(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("google")]
    public async Task<ActionResult<AuthResult>> GoogleLogin(
        [FromBody] GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ExternalLoginCommand(
            AuthProviders.Google,
            request.IdToken,
            Request.Headers.UserAgent.ToString());

        var result = await commandDispatcher.Dispatch<AuthResult>(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenPair>> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshSessionCommand(
            request.RefreshToken,
            Request.Headers.UserAgent.ToString());

        var tokens = await commandDispatcher.Dispatch<TokenPair>(command, cancellationToken);
        return Ok(tokens);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        await commandDispatcher.Dispatch<bool>(new LogoutCommand(request.RefreshToken), cancellationToken);
        return Ok(new { });
    }
}
