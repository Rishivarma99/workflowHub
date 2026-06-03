using Microsoft.Extensions.DependencyInjection;
using WorkflowHub.Application.CQRS.Abstractions;

namespace WorkflowHub.Application.CQRS.Dispatching;

public sealed class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    public async Task<TResponse> Dispatch<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default)
    {
        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResponse));
        var handler = serviceProvider.GetRequiredService(handlerType);

        var handleMethod = handlerType.GetMethod(nameof(ICommandHandler<ICommand<TResponse>, TResponse>.Handle))
            ?? throw new InvalidOperationException($"Handler for {commandType.Name} is missing Handle.");

        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(commandType, typeof(TResponse));
        var behaviors = serviceProvider.GetServices(behaviorType).Cast<object>().ToArray();

        Func<CancellationToken, Task<TResponse>> next = ct =>
        {
            var result = handleMethod.Invoke(handler, [command, ct]);
            return result is Task<TResponse> task
                ? task
                : throw new InvalidOperationException($"Handler for {commandType.Name} returned an invalid task.");
        };

        foreach (var behavior in behaviors.Reverse())
        {
            var current = next;
            var behaviorMethod = behaviorType.GetMethod(nameof(IPipelineBehavior<ICommand<TResponse>, TResponse>.Handle))!;
            next = ct =>
            {
                Task<TResponse> Handler() => current(ct);
                var task = behaviorMethod.Invoke(behavior, [command, ct, (Func<Task<TResponse>>)Handler]) as Task<TResponse>;
                return task ?? throw new InvalidOperationException("Pipeline behavior returned null.");
            };
        }

        return await next(cancellationToken);
    }
}
