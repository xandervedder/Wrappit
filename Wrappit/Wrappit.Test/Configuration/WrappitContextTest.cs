using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using Wrappit.Configuration;

namespace Wrappit.Test.Configuration;

[TestClass]
public class WrappitContextTest
{
    private Mock<IModel> _channelMock = null!;
    private Mock<IConnection> _connectionMock = null!;
    private Mock<IConnectionFactory> _factoryMock = null!;
    private Mock<IWrappitOptions> _optionsMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _channelMock = new Mock<IModel>();
        _connectionMock = new Mock<IConnection>();
        _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);
        _factoryMock = new Mock<IConnectionFactory>();
        _factoryMock.Setup(f => f.CreateConnection()).Returns(_connectionMock.Object);
        _optionsMock = new Mock<IWrappitOptions>();
        _optionsMock.Setup(o => o.CreateFactory()).Returns(_factoryMock.Object);
    }

    [TestMethod]
    public void WrappitContext_WhenCreatingNewContext_ShouldUseVariablesFromOptions()
    {
        // Arrange & Act
        var context = new WrappitContext(new WrappitOptions(), new NullLogger<WrappitContext>());
        
        // Assert
        Assert.AreEqual("Wrappit.DefaultExchangeName", context.ExchangeName);
        Assert.AreEqual("Wrappit.DefaultQueueName", context.QueueName);
        Assert.AreEqual(true, context.DurableQueue);
        Assert.AreEqual(true, context.DurableExchange);
        Assert.AreEqual(false, context.AutoDeleteQueue);
    }

    [TestMethod]
    public void WrappitContext_WhenCreatingChannel_ShouldCreateModelCorrectly()
    {
        // Arrange
        var context = new WrappitContext(_optionsMock.Object, new NullLogger<WrappitContext>());

        // Act
        var channel = context.CreateChannel();

        // Assert
        Assert.AreEqual(_channelMock.Object, channel);
        
        // Once in the .Connection and once in the .CreateModel
        _connectionMock.Verify(c => c.CreateModel(), Times.Exactly(2));
    }

    [TestMethod]
    public void WrappitContext_WhenGettingConnectionMultipleTimes_ShouldCreateExchangeOnce()
    {
        // Arrange
        const string exchangeName = "Wrappit.Exchange";
        _optionsMock.Setup(o => o.ExchangeName).Returns(exchangeName);
        _optionsMock.Setup(o => o.DurableExchange).Returns(true);

        var wrappitContext = new WrappitContext(_optionsMock.Object, new NullLogger<WrappitContext>());

        // Act
        _ = wrappitContext.Connection;
        _ = wrappitContext.Connection;
        _ = wrappitContext.Connection;

        // Assert
        _channelMock.Verify(c => c.ExchangeDeclare(exchangeName, ExchangeType.Topic, true, false, null));
        _factoryMock.Verify(f => f.CreateConnection());
        _connectionMock.Verify(c => c.CreateModel());
    }

    [TestMethod]
    public void WrappitContext_WhenDisposingWithoutCreatedConnection_ShouldNotCloseConnection()
    {
        // Arrange
        var wrappitContext = new WrappitContext(_optionsMock.Object, new NullLogger<WrappitContext>());

        // Act
        wrappitContext.Dispose();
        
        // Assert
        _connectionMock.Verify(c => c.Dispose(), Times.Never);
    }

    [TestMethod]
    public void WrappitContext_WhenDisposingWithConnectionCreated_ShouldCloseConnection()
    {
        // Arrange
        var wrappitContext = new WrappitContext(_optionsMock.Object, new NullLogger<WrappitContext>());

        // Act
        _ = wrappitContext.Connection;
        wrappitContext.Dispose();
        
        // Assert
        _connectionMock.Verify(c => c.Dispose());
    }
}
