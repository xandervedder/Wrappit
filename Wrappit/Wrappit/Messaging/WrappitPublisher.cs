using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Wrappit.Messaging;

internal class WrappitPublisher : IWrappitPublisher
{
    private readonly IBasicSender _sender;
    private readonly ILogger<WrappitPublisher> _logger;

    public WrappitPublisher(IBasicSender sender, ILogger<WrappitPublisher> logger)
    {
        _sender = sender;
        _logger = logger;
    }
    
    public void Publish<T>(string topic, T evt) where T : DomainEvent
    {
        var eventMessage = new EventMessage(topic, JsonConvert.SerializeObject(evt));
        _sender.Send(eventMessage);
        _logger.LogInformation(
            "Published event to topic {topic} with Correlation Id {correlationId}.",
            topic,
            evt.CorrelationId);
    }
}
