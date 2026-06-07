using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowHub.Api.Contracts.Requests.Learn;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Learn.Queries.GetArticleBySlug;
using WorkflowHub.Application.Features.Learn.Queries.GetLearnHome;
using WorkflowHub.Application.Features.Learn.Queries.SearchArticles;
using WorkflowHub.Application.Features.Learn.ReadModels;
using WorkflowHub.Common.Responses;

namespace WorkflowHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/articles")]
public sealed class ArticlesController(IQueryDispatcher queryDispatcher) : ControllerBase
{
    [HttpGet("home")]
    public async Task<ActionResult<LearnHomeDto>> GetHome(CancellationToken cancellationToken = default)
    {
        var result = await queryDispatcher.Dispatch<LearnHomeDto>(
            new GetLearnHomeQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("search")]
    public async Task<ActionResult<PagedResult<ArticleCardDto>>> Search(
        [FromBody] SearchArticlesRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await queryDispatcher.Dispatch<PagedResult<ArticleCardDto>>(
            new SearchArticlesQuery(
                request.Query,
                request.Page,
                request.PageSize,
                request.Filters ?? new Dictionary<string, string>()),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ArticleDetailDto>> GetBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var result = await queryDispatcher.Dispatch<ArticleDetailDto?>(
            new GetArticleBySlugQuery(slug),
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
