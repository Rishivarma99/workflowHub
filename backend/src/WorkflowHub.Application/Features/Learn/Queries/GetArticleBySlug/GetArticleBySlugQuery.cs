using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Learn.ReadModels;

namespace WorkflowHub.Application.Features.Learn.Queries.GetArticleBySlug;

public sealed record GetArticleBySlugQuery(string Slug) : IQuery<ArticleDetailDto?>;
