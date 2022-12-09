using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Wrappit.Configuration;

namespace Wrappit.Messaging;

internal class WrappitPublisher : IWrappitPublisher
{
    private readonly IBasicSender _sender;

    public WrappitPublisher(IWrappitContext context, ILogger<BasicSender> logger) : this(new BasicSender(context, logger))
    {
    }
    
    internal WrappitPublisher(IBasicSender sender)
    {
        _sender = sender;
    }

    public void Publish<T>(string topic, T evt) where T : DomainEvent
    {
        var eventMessage = new EventMessage(topic, JsonConvert.SerializeObject(evt));
        _sender.Send(eventMessage);
    }
}
