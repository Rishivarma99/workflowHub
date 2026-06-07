using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Learn.ReadModels;

namespace WorkflowHub.Application.Features.Learn.Queries.GetLearnHome;

public sealed record GetLearnHomeQuery : IQuery<LearnHomeDto>;
