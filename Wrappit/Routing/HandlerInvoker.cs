using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Wrappit.Messaging;

namespace Wrappit.Routing;

// TODO: StatefullHandlerInvoker & StatelessHandlerInvoker
internal class HandlerInvoker : IHandlerInvoker
{
    public Type EventListenerType { get; }
    public string Topic { get; }
    public MethodInfo HandlerMethod { get; }
    public Type ParameterType { get; }
    
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HandlerInvoker> _logger;
    
    public HandlerInvoker(
        Type eventListenerType, 
        MethodInfo handlerMethod, 
        Type parameterType, 
        string topic, 
        IServiceProvider serviceProvider,
        ILogger<HandlerInvoker> logger)
    {
        EventListenerType = eventListenerType;
        HandlerMethod = handlerMethod;
        ParameterType = parameterType;
        Topic = topic;
        
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void Dispatch(EventMessage message)
    {
        var instance = ActivatorUtilities.CreateInstance(_serviceProvider, EventListenerType);
        var eventInstance = JsonConvert.DeserializeObject(message.Body, ParameterType);

        if (HandlerMethod.ReturnType == typeof(Task))
        {
            Task.Run(() => InvokeMethod(instance, eventInstance));
            _logger.LogDebug(
                "Invoked method {method} of instance {instance} asynchronously.", 
                HandlerMethod.Name, 
                EventListenerType.Name);
            return;
        }
        
        InvokeMethod(instance, eventInstance);
        _logger.LogDebug(
            "Invoked method {method} of instance {instance} synchronously.", 
            HandlerMethod.Name, 
            EventListenerType.Name);
    }

    private void InvokeMethod(object instance, object eventInstance)
    {
        HandlerMethod.Invoke(instance, new []{ eventInstance });
    }
}
