using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Discover.ReadModels;

namespace WorkflowHub.Application.Features.Discover.Queries.GetWorkflowDetail;

public sealed record GetWorkflowDetailQuery(Guid WorkflowId, Guid? UserId) : IQuery<WorkflowDetailDto?>;
