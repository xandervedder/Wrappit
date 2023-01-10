using RabbitMQ.Client;

namespace Wrappit.Configuration;

public class WrappitOptions : IWrappitOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "Wrappit.DefaultExchangeName";
    public bool DurableExchange {get; set; } = true;
    public string QueueName { get; set; } = "Wrappit.DefaultQueueName";
    public int DeliveryLimit { get; set; } = 10;
    public bool DurableQueue { get; set; } = true;
    public bool AutoDeleteQueue { get; set; } = false;

    public IConnectionFactory CreateFactory()
    {
        return new ConnectionFactory
        {
            HostName = HostName,
            Port = Port,
            UserName = UserName,
            Password = Password,
        };
    }
}
