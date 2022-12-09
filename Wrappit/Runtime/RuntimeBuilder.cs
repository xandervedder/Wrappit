using System.Reflection;
using Microsoft.Extensions.Logging;
using Wrappit.Attributes;
using Wrappit.Exceptions;
using Wrappit.Messaging;
using Wrappit.Routing;

namespace Wrappit.Runtime;

internal class RuntimeBuilder : IRuntimeBuilder
{
    private readonly ITypeFinder _typeFinder;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<IHandlerInvoker> _handlers;
    
    private readonly ILogger<RuntimeBuilder> _runtimeLogger;
    private readonly ILogger<HandlerInvoker> _handlerLogger;
    private readonly ILogger<WrappitRouter> _routerLogger;
 
    public RuntimeBuilder(
        ITypeFinder typeFinder,
        IServiceProvider serviceProvider,
        ILogger<RuntimeBuilder> runtimeLogger,
        ILogger<HandlerInvoker> handlerLogger,
        ILogger<WrappitRouter> routerLogger)
    {
        _typeFinder = typeFinder;
        _serviceProvider = serviceProvider;
        _runtimeLogger = runtimeLogger;
        _handlerLogger = handlerLogger;
        _routerLogger = routerLogger;
        _handlers = new List<IHandlerInvoker>();
    }

    public IRuntimeBuilder RegisterEventListener(Type eventListenerType)
    {
        var methods = GetMethodsWithAttribute(eventListenerType);
        foreach (var method in methods)
        {
            var parameters = method.GetParameters();
            if (parameters.Length is > 1 or 0)
            {
                throw new WrappitException($"Method signature of {method.Name} is invalid, please provide exactly one parameter.");
            }

            var parameter = parameters.First();
            if (!parameter.ParameterType.IsSubclassOf(typeof(DomainEvent)))
            {
                throw new WrappitException($"Parameter type should derive from {nameof(DomainEvent)}.");
            }
            var attribute = method.GetCustomAttribute<HandleAttribute>()!;
            var handler = new HandlerInvoker(eventListenerType, method, parameter.ParameterType, attribute.Topic, _serviceProvider, _handlerLogger);
            _handlers.Add(handler);
            _runtimeLogger.LogDebug(
                "Event listener class {listener} registered with method {method}.", 
                handler.EventListenerType.Name, 
                handler.HandlerMethod.Name);
        }
        
        return this;
    }

    private static IEnumerable<MethodInfo> GetMethodsWithAttribute(Type eventListenerType)
    {
        return eventListenerType.GetMethods().Where(m => m.GetCustomAttributes<HandleAttribute>().Any());
    }

    public IRuntimeBuilder DiscoverAndRegisterAllEventListeners()
    {
        var types = _typeFinder.FindAllTypes()
            .Where(t => t.IsClass && t.GetCustomAttribute<EventListenerAttribute>() != null);
        
        foreach (var type in types)
        {
            RegisterEventListener(type);
        }
        
        _runtimeLogger.LogInformation("{handlers} event listeners registered", _handlers.Count);
        
        return this;
    }

    public IWrappitRouter Build()
    {
        return new WrappitRouter(_handlers, _routerLogger);
    }
}
