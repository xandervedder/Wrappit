using RabbitMQ.Client;

namespace Wrappit.Configuration;

public interface IWrappitContext : IDisposable
{
    public IConnection Connection { get; }
    public string ExchangeName { get; }
    public string QueueName { get; }

    public IModel CreateChannel();
}
