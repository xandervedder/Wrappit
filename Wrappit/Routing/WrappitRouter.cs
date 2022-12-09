using Microsoft.Extensions.Logging;
using Wrappit.Messaging;

namespace Wrappit.Routing;

internal class WrappitRouter : IWrappitRouter
{
    public IEnumerable<string> Topics { get; }
    public IEnumerable<IHandlerInvoker> Handlers { get; }

    private readonly ILogger<WrappitRouter> _logger;

    public WrappitRouter(IEnumerable<IHandlerInvoker> handlers, ILogger<WrappitRouter> logger)
    {
        Handlers = handlers;
        Topics = handlers.Select(h => h.Topic);

        _logger = logger;
    }
    
    public void Route(EventMessage eventMessage)
    {
        _logger.LogInformation("Routing message with routing key {key}.", eventMessage.Topic);
        
        foreach (var handler in Handlers.Where(h => h.Topic == eventMessage.Topic))
        {
            handler.Dispatch(eventMessage);
        }
    }
}
