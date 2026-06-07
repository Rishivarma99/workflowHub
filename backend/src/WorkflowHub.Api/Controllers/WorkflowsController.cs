using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowHub.Api.Contracts.Requests.Discover;
using WorkflowHub.Api.Contracts.Requests.Install;
using WorkflowHub.Api.Contracts.Requests.Publish;
using WorkflowHub.Application.Contracts.Identity;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Discover.ReadModels;
using WorkflowHub.Application.Features.Discover.Queries.BrowseWorkflows;
using WorkflowHub.Application.Features.Discover.Queries.GetDiscoverHome;
using WorkflowHub.Application.Features.Discover.Queries.GetWorkflowDetail;
using WorkflowHub.Application.Features.Discover.Queries.SearchWorkflows;
using WorkflowHub.Application.Features.Engagement.Commands.RecordWorkflowDownload;
using WorkflowHub.Application.Features.Engagement.Commands.StarWorkflow;
using WorkflowHub.Application.Features.Engagement.Commands.UnstarWorkflow;
using WorkflowHub.Application.Features.Engagement.ReadModels;
using WorkflowHub.Application.Features.Install.Commands.GenerateInstallPrompt;
using WorkflowHub.Application.Features.Install.ReadModels;
using WorkflowHub.Application.Features.MyWorkflows.Commands.DeleteWorkflow;
using WorkflowHub.Application.Features.MyWorkflows.Queries.GetMyWorkflows;
using WorkflowHub.Application.Features.MyWorkflows.ReadModels;
using WorkflowHub.Application.Features.Publish.Commands.PublishWorkflow;
using WorkflowHub.Application.Features.Publish.Queries.CheckWorkflowName;
using WorkflowHub.Application.Features.Publish.Queries.ValidateRepository;
using WorkflowHub.Common.Responses;

namespace WorkflowHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/workflows")]
public sealed class WorkflowsController(
    ICurrentUserAccessor currentUser,
    IQueryDispatcher queryDispatcher,
    ICommandDispatcher commandDispatcher) : ControllerBase
{
    [HttpGet("home")]
    public async Task<ActionResult<DiscoverHomeDto>> GetHome(CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId(User);

        var result = await queryDispatcher.Dispatch<DiscoverHomeDto>(
            new GetDiscoverHomeQuery(userId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("browse")]
    public async Task<ActionResult<PagedResult<WorkflowCardDto>>> Browse(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? types = null,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.GetUserId(User);

        var result = await queryDispatcher.Dispatch<PagedResult<WorkflowCardDto>>(
            new BrowseWorkflowsQuery(userId, page, pageSize, types),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("search")]
    public async Task<ActionResult<PagedResult<WorkflowCardDto>>> Search(
        [FromBody] SearchWorkflowsRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.GetUserId(User);

        var result = await queryDispatcher.Dispatch<PagedResult<WorkflowCardDto>>(
            new SearchWorkflowsQuery(
                userId,
                request.Query,
                request.Page,
                request.PageSize,
                request.SortBy,
                request.Filters),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<MyWorkflowsSummaryDto>> GetMine(CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await queryDispatcher.Dispatch<MyWorkflowsSummaryDto>(
            new GetMyWorkflowsQuery(userId.Value),
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{workflowId:guid}")]
    public async Task<IActionResult> Delete(Guid workflowId, CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        await commandDispatcher.Dispatch<bool>(
            new DeleteWorkflowCommand(userId.Value, workflowId),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{workflowId:guid}")]
    public async Task<ActionResult<WorkflowDetailDto>> GetById(
        Guid workflowId,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId(User);

        var result = await queryDispatcher.Dispatch<WorkflowDetailDto?>(
            new GetWorkflowDetailQuery(workflowId, userId),
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost("{workflowId:guid}/star")]
    public async Task<ActionResult<StarActionResultDto>> Star(
        Guid workflowId,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await commandDispatcher.Dispatch<StarActionResultDto>(
            new StarWorkflowCommand(userId.Value, workflowId),
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{workflowId:guid}/star")]
    public async Task<ActionResult<StarActionResultDto>> Unstar(
        Guid workflowId,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await commandDispatcher.Dispatch<StarActionResultDto>(
            new UnstarWorkflowCommand(userId.Value, workflowId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{workflowId:guid}/install-prompt")]
    public async Task<ActionResult<GenerateInstallPromptResultDto>> GenerateInstallPrompt(
        Guid workflowId,
        [FromBody] GenerateInstallPromptRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Dispatch<GenerateInstallPromptResultDto>(
            new GenerateInstallPromptCommand(
                workflowId,
                request.TargetAgent,
                request.InstallLevel),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{workflowId:guid}/download")]
    public async Task<ActionResult<RecordWorkflowDownloadResultDto>> RecordDownload(
        Guid workflowId,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await commandDispatcher.Dispatch<RecordWorkflowDownloadResultDto>(
            new RecordWorkflowDownloadCommand(userId.Value, workflowId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("validate-repo")]
    public async Task<ActionResult<RepoValidationDto>> ValidateRepository(
        [FromBody] ValidateRepositoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await queryDispatcher.Dispatch<RepoValidationDto>(
            new ValidateRepositoryQuery(request.RepositoryUrl),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("check-name")]
    public async Task<ActionResult<NameAvailabilityDto>> CheckName(
        [FromQuery] string name,
        CancellationToken cancellationToken)
    {
        var result = await queryDispatcher.Dispatch<NameAvailabilityDto>(
            new CheckWorkflowNameQuery(name),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PublishWorkflowResultDto>> Publish(
        [FromBody] PublishWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var builtForAgents = ResolveBuiltForAgents(request);
        var analysis = MapAnalysis(request.Analysis);
        var command = new PublishWorkflowCommand(
            userId.Value,
            request.RepositoryUrl,
            builtForAgents,
            request.WorkflowName,
            request.Description,
            request.Tags,
            request.Dependencies
                .Select(d => new PublishDependencyDto(d.Kind, d.Name, d.Requirement, d.Note))
                .ToList(),
            request.Complexity,
            request.TargetAudience,
            analysis);

        var result = await commandDispatcher.Dispatch<PublishWorkflowResultDto>(command, cancellationToken);
        return Ok(result);
    }

    private static IReadOnlyList<string> ResolveBuiltForAgents(PublishWorkflowRequest request)
    {
        if (request.BuiltForAgents.Count > 0)
        {
            return request.BuiltForAgents;
        }

        if (!string.IsNullOrWhiteSpace(request.SourceIde))
        {
            return [request.SourceIde];
        }

        return [];
    }

    private static PublishAnalysisDto MapAnalysis(PublishAnalysisRequest analysis) =>
        new(
            analysis.WorkflowName,
            analysis.Description,
            analysis.Tags,
            analysis.SuggestedDependencies,
            analysis.Components
                .Select(c => new PublishAnalysisComponentDto(
                    c.Path,
                    c.ComponentType,
                    c.Title,
                    c.Summary,
                    c.Capabilities
                        .Select(cap => new PublishAnalysisCapabilityDto(cap.Name, cap.Description))
                        .ToList(),
                    c.Keywords,
                    c.SearchPhrases,
                    c.Technologies,
                    c.Dependencies))
                .ToList());
}
