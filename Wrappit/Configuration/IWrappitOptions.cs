using RabbitMQ.Client;

namespace Wrappit.Configuration;

public interface IWrappitOptions
{
    public string HostName { get; set; }
    public int Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
 
    public string ExchangeName { get; set; }
    public string QueueName { get; set; }

    public IConnectionFactory CreateFactory();
}
