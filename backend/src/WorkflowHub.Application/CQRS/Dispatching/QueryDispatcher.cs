using Microsoft.Extensions.DependencyInjection;
using WorkflowHub.Application.CQRS.Abstractions;

namespace WorkflowHub.Application.CQRS.Dispatching;

public sealed class QueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
    public async Task<TResponse> Dispatch<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default)
    {
        var queryType = query.GetType();
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResponse));
        var handler = serviceProvider.GetRequiredService(handlerType);

        var handleMethod = handlerType.GetMethod(nameof(IQueryHandler<IQuery<TResponse>, TResponse>.Handle))
            ?? throw new InvalidOperationException($"Handler for {queryType.Name} is missing Handle.");

        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(queryType, typeof(TResponse));
        var behaviors = serviceProvider.GetServices(behaviorType).Cast<object>().ToArray();

        Func<CancellationToken, Task<TResponse>> next = ct =>
        {
            var result = handleMethod.Invoke(handler, [query, ct]);
            return result is Task<TResponse> task
                ? task
                : throw new InvalidOperationException($"Handler for {queryType.Name} returned an invalid task.");
        };

        foreach (var behavior in behaviors.Reverse())
        {
            var current = next;
            var behaviorMethod = behaviorType.GetMethod(nameof(IPipelineBehavior<IQuery<TResponse>, TResponse>.Handle))!;
            next = ct =>
            {
                Task<TResponse> Handler() => current(ct);
                var task = behaviorMethod.Invoke(behavior, [query, ct, (Func<Task<TResponse>>)Handler]) as Task<TResponse>;
                return task ?? throw new InvalidOperationException("Pipeline behavior returned null.");
            };
        }

        return await next(cancellationToken);
    }
}
