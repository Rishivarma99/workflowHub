using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.AgentAssets.ReadModels;

namespace WorkflowHub.Application.Features.AgentAssets.Queries.GetAgentAssetsHome;

public sealed record GetAgentAssetsHomeQuery(Guid? UserId) : IQuery<AgentAssetsHomeDto>;
