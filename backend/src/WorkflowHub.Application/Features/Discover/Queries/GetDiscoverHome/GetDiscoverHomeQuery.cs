using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.Features.Discover.ReadModels;

namespace WorkflowHub.Application.Features.Discover.Queries.GetDiscoverHome;

public sealed record GetDiscoverHomeQuery(Guid? UserId) : IQuery<DiscoverHomeDto>;
