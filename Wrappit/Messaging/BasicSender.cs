using System.Text;
using Microsoft.Extensions.Logging;
using Wrappit.Configuration;

namespace Wrappit.Messaging;

internal class BasicSender : IBasicSender
{
    private readonly IWrappitContext _context;
    private readonly ILogger<BasicSender> _logger;

    public BasicSender(IWrappitContext context, ILogger<BasicSender> logger)
    {
        _context = context;
        _logger = logger;
    }

    public void Send(EventMessage message)
    {
        using var channel = _context.CreateChannel();
        var encoded = Encoding.Unicode.GetBytes(message.Body);
        channel.BasicPublish(_context.ExchangeName, message.Topic, true, null, encoded);
        _logger.LogDebug(
            "Message sent to exchange {exchangeName} with topic {topic}.", 
            _context.ExchangeName,
            message.Topic);
    }
}
