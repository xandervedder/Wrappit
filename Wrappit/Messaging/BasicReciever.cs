using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Wrappit.Configuration;
using Wrappit.Exceptions;

namespace Wrappit.Messaging;

internal class BasicReciever : IBasicReciever
{
    private readonly IWrappitContext _context;
    private readonly ILogger<IBasicReciever> _logger;

    private IModel? _channel;
    private bool _queueDeclared;
    private bool _isRecieving;
    
    public BasicReciever(IWrappitContext context, ILogger<IBasicReciever> logger)
    {
        _context = context;
        _logger = logger;
    }

    public void SetUpQueue(IEnumerable<string> topics)
    {
        var topicsList = CanSetUpQueue(topics);

        using var channel = _context.CreateChannel();
        channel.QueueDeclare(_context.QueueName, true, false, false, null);
        _logger.LogDebug("Queue with name {name} set up.", _context.QueueName);
        
        foreach (var topic in topicsList)
        {
            channel.QueueBind(_context.QueueName, _context.ExchangeName, topic, null);
            _logger.LogDebug("Binding added to Queue {queueName} with topic {topic}.", _context.QueueName, topic);
        }

        _queueDeclared = true;
    }

    private List<string> CanSetUpQueue(IEnumerable<string> topics)
    {
        if (_queueDeclared)
        {
            throw new WrappitException("Queue is already declared.");
        }

        var topicsList = topics.ToList();
        if (topicsList == null || !topicsList.Any())
        {
            throw new WrappitException("Atleast one topic should be provided.");
        }

        return topicsList;
    }

    public void StartRecieving(Action<EventMessage> handler)
    {
        CanStartRecieving();

        _channel = _context.CreateChannel();
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (_, args) =>
        {
            var bytes = args.Body.ToArray();
            var eventMessage = new EventMessage(args.RoutingKey, Encoding.Unicode.GetString(bytes));

            try
            {
                handler(eventMessage);
                _channel.BasicAck(args.DeliveryTag, true);
                _logger.LogDebug("Message with routing key {key} acknowledged.", eventMessage.Topic);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogCritical("Dependency injection failed.\n{exception}", e.Message);
                throw new WrappitException("Dependency injection failed.", e);
            }
            catch (Exception e)
            {
                _channel.BasicNack(args.DeliveryTag, false, true);
                _logger.LogError(
                    "Could not acknowledge message with routing key {key}. Reason:\n{exception}",
                    eventMessage.Topic,
                    e.Message);
            }
        };
        
        _channel.BasicConsume(_context.QueueName, false, ToString(), false, false, null, consumer);
        _isRecieving = true;
    }

    private void CanStartRecieving()
    {
        if (!_queueDeclared)
        {
            throw new WrappitException("Queue has not been declared yet (hint: call `.SetUpQueue()`).");
        }

        if (_isRecieving)
        {
            throw new WrappitException("Cannot listen multiple times.");
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}
