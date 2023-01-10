using RabbitMQ.Client;
using Wrappit.Configuration;

namespace Wrappit.Test.Configuration;

[TestClass]
public class WrappitOptionsTest
{
    [TestMethod]
    public void WrappitOptions_WhenCreatingIt_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var options = new WrappitOptions();

        // Assert
        Assert.AreEqual("guest", options.UserName);
        Assert.AreEqual("guest", options.Password);
        Assert.AreEqual("localhost", options.HostName);
        Assert.AreEqual(5672, options.Port);
        Assert.AreEqual("Wrappit.DefaultExchangeName", options.ExchangeName);
        Assert.AreEqual("Wrappit.DefaultQueueName", options.QueueName);
        Assert.AreEqual(10, options.DeliveryLimit);
        Assert.AreEqual(true, options.DurableQueue);
        Assert.AreEqual(true, options.DurableExchange);
        Assert.AreEqual(false, options.AutoDeleteQueue);
    }

    [TestMethod]
    public void WrappitOptions_WhenCreatingItWithValues_ShouldHaveValues()
    {
        // Arrange & Act
        var options = new WrappitOptions
        {
            UserName = "Evert",
            Password = "'t Reve",
            Port = 1200,
            HostName = "wrappit.org",
            ExchangeName = "Wrappit.Exchange",
            QueueName = "Wrappit.Queue",
            DeliveryLimit = 1,
            DurableQueue = false,
            DurableExchange = false,
            AutoDeleteQueue = true,
        };

        // Assert
        Assert.AreEqual("Evert", options.UserName);
        Assert.AreEqual("'t Reve", options.Password);
        Assert.AreEqual("wrappit.org", options.HostName);
        Assert.AreEqual(1200, options.Port);
        Assert.AreEqual("Wrappit.Exchange", options.ExchangeName);
        Assert.AreEqual("Wrappit.Queue", options.QueueName);
        Assert.AreEqual(1, options.DeliveryLimit);
        Assert.AreEqual(false, options.DurableQueue);
        Assert.AreEqual(false, options.DurableExchange);
        Assert.AreEqual(true, options.AutoDeleteQueue);
    }

    [TestMethod]
    public void WrappitOptions_WithDefaultValues_ShouldCreateConnectionFactoryCorrectly()
    {
        // Arrange
        var options = new WrappitOptions();
        
        // Act
        var factory = (ConnectionFactory) options.CreateFactory();

        // Assert
        Assert.AreEqual("guest", factory.UserName);
        Assert.AreEqual("guest", factory.Password);
        Assert.AreEqual("localhost", factory.HostName);
        Assert.AreEqual(5672, factory.Port);
    }
}
