using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Wrappit.Configuration;

namespace Wrappit.Messaging;

internal class WrappitPublisher : IWrappitPublisher
{
    private readonly IBasicSender _sender;
    private readonly ILogger<WrappitPublisher> _logger;

    public WrappitPublisher(IWrappitContext context, ILogger<BasicSender> senderLogger, ILogger<WrappitPublisher> publisherLogger) : this(new BasicSender(context, senderLogger), publisherLogger)
    {
    }
    
    internal WrappitPublisher(IBasicSender sender, ILogger<WrappitPublisher> publisherLogger)
    {
        _sender = sender;
        _logger = publisherLogger;
    }

    public void Publish<T>(string topic, T evt) where T : DomainEvent
    {
        var eventMessage = new EventMessage(topic, JsonConvert.SerializeObject(evt));
        _sender.Send(eventMessage);
        _logger.LogDebug(
            "Published event to topic {topic} with Correlation Id {correlationId}",
            topic,
            evt.CorrelationId);
    }
}
