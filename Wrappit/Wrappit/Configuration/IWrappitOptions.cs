using RabbitMQ.Client;

namespace Wrappit.Configuration;

public interface IWrappitOptions
{
    public string HostName { get; set; }
    public int Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
 
    public string ExchangeName { get; set; }
    public bool DurableExchange {get; set; }
    public string QueueName { get; set; }
    public int DeliveryLimit { get; set; }
    public bool DurableQueue { get; set; }
    public bool AutoDeleteQueue { get; set; }

    public IConnectionFactory CreateFactory();
}
