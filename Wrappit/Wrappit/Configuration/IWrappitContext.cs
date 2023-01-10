using RabbitMQ.Client;

namespace Wrappit.Configuration;

public interface IWrappitContext : IDisposable
{
    public IConnection Connection { get; }
    
    public string ExchangeName { get; }
    public bool DurableExchange {get; }
    public string QueueName { get; }
    public int DeliveryLimit { get; }
    public bool DurableQueue { get; }
    public bool AutoDeleteQueue { get; }

    public IModel CreateChannel();
}
