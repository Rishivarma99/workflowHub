namespace WorkflowHub.Application.CQRS.Abstractions;

public interface IQueryDispatcher
{
    Task<TResponse> Dispatch<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default);
}
