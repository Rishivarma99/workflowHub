namespace WorkflowHub.Application.CQRS.Abstractions;

public interface ICommandDispatcher
{
    Task<TResponse> Dispatch<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default);
}
