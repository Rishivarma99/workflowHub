using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WorkflowHub.Application.CQRS.Abstractions;
using WorkflowHub.Application.CQRS.Behaviors;
using WorkflowHub.Application.CQRS.Dispatching;

namespace WorkflowHub.Application.Bootstrap;

internal static class CqrsRegistrationExtensions
{
    public static IServiceCollection AddCqrs(this IServiceCollection services, Assembly assembly)
    {
        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        RegisterHandlers(services, assembly, typeof(ICommandHandler<,>));
        RegisterHandlers(services, assembly, typeof(IQueryHandler<,>));

        return services;
    }

    private static void RegisterHandlers(
        IServiceCollection services,
        Assembly assembly,
        Type handlerOpenGeneric)
    {
        foreach (var type in assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }))
        {
            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType || iface.GetGenericTypeDefinition() != handlerOpenGeneric)
                {
                    continue;
                }

                services.AddScoped(iface, type);
            }
        }
    }
}
