using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowHub.Api.Contracts.Requests.AgentAssets;
using WorkflowHub.Application.Contracts.Identity;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.AgentAssets.Queries.BrowseAgentAssets;
using WorkflowHub.Application.Features.AgentAssets.Queries.GetAgentAssetsHome;
using WorkflowHub.Application.Features.AgentAssets.Queries.SearchAgentAssets;
using WorkflowHub.Application.Features.AgentAssets.ReadModels;
using WorkflowHub.Application.Features.Engagement.Commands.StarAgentAsset;
using WorkflowHub.Application.Features.Engagement.Commands.UnstarAgentAsset;
using WorkflowHub.Application.Features.Engagement.ReadModels;
using WorkflowHub.Common.Responses;

namespace WorkflowHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/agent-assets")]
public sealed class AgentAssetsController(
    ICurrentUserAccessor currentUser,
    IQueryDispatcher queryDispatcher,
    ICommandDispatcher commandDispatcher) : ControllerBase
{
    [HttpGet("home")]
    public async Task<ActionResult<AgentAssetsHomeDto>> GetHome(CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId(User);

        var result = await queryDispatcher.Dispatch<AgentAssetsHomeDto>(
            new GetAgentAssetsHomeQuery(userId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("browse")]
    public async Task<ActionResult<PagedResult<AgentAssetCardDto>>> Browse(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? types = null,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.GetUserId(User);

        var result = await queryDispatcher.Dispatch<PagedResult<AgentAssetCardDto>>(
            new BrowseAgentAssetsQuery(userId, page, pageSize, types),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("search")]
    public async Task<ActionResult<PagedResult<AgentAssetCardDto>>> Search(
        [FromBody] SearchAgentAssetsRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.GetUserId(User);

        var result = await queryDispatcher.Dispatch<PagedResult<AgentAssetCardDto>>(
            new SearchAgentAssetsQuery(
                userId,
                request.Query,
                request.Page,
                request.PageSize,
                request.SortBy,
                request.Filters),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{agentAssetId:guid}/star")]
    public async Task<ActionResult<StarActionResultDto>> Star(
        Guid agentAssetId,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await commandDispatcher.Dispatch<StarActionResultDto>(
            new StarAgentAssetCommand(userId.Value, agentAssetId),
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{agentAssetId:guid}/star")]
    public async Task<ActionResult<StarActionResultDto>> Unstar(
        Guid agentAssetId,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await commandDispatcher.Dispatch<StarActionResultDto>(
            new UnstarAgentAssetCommand(userId.Value, agentAssetId),
            cancellationToken);

        return Ok(result);
    }
}
