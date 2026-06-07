using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.MyWorkflows.ReadModels;

namespace WorkflowHub.Application.Features.MyWorkflows.Queries.GetMyWorkflows;

public sealed record GetMyWorkflowsQuery(Guid OwnerId) : IQuery<MyWorkflowsSummaryDto>;
