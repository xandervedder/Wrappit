using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Wrappit.Configuration;
using Wrappit.Exceptions;
using Wrappit.Messaging;

namespace Wrappit.Test.Messaging;

[TestClass]
public class BasicRecieverTest
{
    private const string ExchangeName = "Wrappit.Exchange";
    private const string QueueName = "Wrappit.Queue";

    private Mock<IModel> _channelMock = null!;
    private Mock<IConnection> _connectionMock = null!;
    private Mock<IWrappitContext> _contextMock = null!;
    private BasicReciever _reciever = null!;
    
    [TestInitialize]
    public void Setup()
    {
        _channelMock = new Mock<IModel>();
        _connectionMock = new Mock<IConnection>();
        _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);
        _contextMock = new Mock<IWrappitContext>();
        _contextMock.Setup(c => c.ExchangeName).Returns(ExchangeName);
        _contextMock.Setup(c => c.QueueName).Returns(QueueName);
        _contextMock.Setup(c => c.CreateChannel()).Returns(_channelMock.Object);
        _reciever = new BasicReciever(_contextMock.Object, new NullLogger<BasicReciever>());
    }
    
    [TestMethod]
    public void BasicReciever_WhenSettingUpQueueWithTopics_ShouldCreateAndBindQueue()
    {
        // Arrange
        const string topic = "Wrappit.Topic";

        // Act
        _reciever.SetUpQueue(new [] { topic });

        // Assert
        _channelMock.Verify(c => c.QueueDeclare(QueueName, true, false, false, null));
        _channelMock.Verify(c => c.QueueBind(QueueName, ExchangeName, topic, null));
    }

    [TestMethod]
    public void BasicReciever_WhenSettingUpQueueTwice_ShouldThrowException()
    {
        // Arrange
        const string topic = "Wrappit.Topic";

        // Act
        _reciever.SetUpQueue(new [] { topic });
        void SetUpQueueSecondTime() => _reciever.SetUpQueue(new[] { topic });

        // Assert
        var exception = Assert.ThrowsException<WrappitException>(SetUpQueueSecondTime);
        Assert.AreEqual("Queue is already declared.", exception.Message);
    }

    [TestMethod]
    public void BasicReciever_WhenSettingUpQueueWithMultipleTopics_ShouldCreateMultipleBindings()
    {
        // Arrange
        var topics = new [] { "Wrappit.Topic1", "Wrappit.Topic2", "Wrappit.Topic3" };

        // Act
        _reciever.SetUpQueue(topics);

        // Assert
        // Should only contain one QueueDeclare:
        _channelMock.Verify(c => c.QueueDeclare(QueueName, true, false, false, null));
        _channelMock.Verify(c => c.QueueBind(QueueName, ExchangeName, It.IsAny<string>(), null), Times.Exactly(3));
    }
    
    [TestMethod]
    public void BasicReciever_WhenSettingUpQueueWithNoTopics_ShouldThrowException()
    {
        // Arrange & Act
        void SetUpQueue() => _reciever.SetUpQueue(Array.Empty<string>());

        // Assert
        var exception = Assert.ThrowsException<WrappitException>(SetUpQueue);
        Assert.AreEqual("Atleast one topic should be provided.", exception.Message);
    }

    [TestMethod]
    public void BasicReciever_WhenSettingUpQueueWithNoBrokerAvailable_ShouldThrowException()
    {
        // Arrange
        // Use *real* implementation to get it to throw an exception.
        var reciever = new BasicReciever(new WrappitContext(new WrappitOptions(), new NullLogger<WrappitContext>()), new NullLogger<BasicReciever>());

        // Act
        void SetUpQueue() => reciever.SetUpQueue(new []{ "MyTopic" });

        // Assert
        Assert.ThrowsException<BrokerUnreachableException>(SetUpQueue);
    }

    [TestMethod]
    public void BasicReciever_WhenRecievingBeforeQueueSetup_ShouldThrowException()
    {
        // Arrange & Act
        void StartRecieving() => _reciever.StartRecieving(_ => {});

        // Assert
        var exception = Assert.ThrowsException<WrappitException>(StartRecieving);
        Assert.AreEqual("Queue has not been declared yet (hint: call `.SetUpQueue()`).", exception.Message);
    }

    [TestMethod]
    public void BasicReciever_WhenTryingToStartRecievingTwice_ShouldThrowException()
    {
        // Arrange & Act
        _reciever.SetUpQueue(new[] { "Wrappit.Topic" });
        _reciever.StartRecieving(_ => {});
        
        void StartRecieving() => _reciever.StartRecieving(_ => {});

        // Assert
        var exception = Assert.ThrowsException<WrappitException>(StartRecieving);
        Assert.AreEqual("Cannot listen multiple times.", exception.Message);
    }

    [TestMethod]
    public void BasicReciever_WhenAMessageIsDelivered_ShouldHaveCorrectEventMessage()
    {
        // Arrange
        EventingBasicConsumer? consumer = null;
        string? body = null;

        _channelMock
            .Setup(c => c.BasicConsume(QueueName, false, It.IsAny<string>(), false, false, null, It.IsAny<EventingBasicConsumer>()))
            .Callback<string, bool, string, bool, bool, IDictionary<string, object>, IBasicConsumer>((_, _, _, _, _, _, e) => consumer = (EventingBasicConsumer)e);

        // Act
        _reciever.SetUpQueue(new []{ "Wrappit.Topic" });
        _reciever.StartRecieving(message => body = message.Body);

        consumer?.HandleBasicDeliver("", 0, false, ExchangeName, "Wrappit.Topic", null, Encoding.Unicode.GetBytes("Hello World!"));
        
        // Assert
        Assert.AreEqual("Hello World!", body);
        _channelMock.Verify(c => c.BasicConsume(QueueName, false, It.IsAny<string>(), false, false, null, It.IsAny<EventingBasicConsumer>()));
    }
    
    [TestMethod]
    public void BasicReciever_WhenStartingRecievingWithNoBrokerAvailable_ShouldThrowException()
    {
        // Arrange & Act
        _reciever.SetUpQueue(new []{ "MyTopic" });
        
        _contextMock.Setup(c => c.CreateChannel())
            .Throws(new BrokerUnreachableException(null));
        void StartRecieving() => _reciever.StartRecieving(_ => {});
    
        // Assert
        Assert.ThrowsException<BrokerUnreachableException>(StartRecieving);
    }

    [TestMethod]
    public void BasicReciever_WhenStartingRecievingMessage_ShouldSendAck()
    {
        // Arrange
        EventingBasicConsumer? consumer = null;

        _channelMock
            .Setup(c => c.BasicConsume(QueueName, false, It.IsAny<string>(), false, false, null, It.IsAny<EventingBasicConsumer>()))
            .Callback<string, bool, string, bool, bool, IDictionary<string, object>, IBasicConsumer>((_, _, _, _, _, _, e) => consumer = (EventingBasicConsumer)e);

        // Act
        _reciever.SetUpQueue(new []{ "Wrappit.Topic" });
        _reciever.StartRecieving(_ => { /* Code that does not throw. */ });
        
        consumer?.HandleBasicDeliver("", 0, false, ExchangeName, "Wrappit.Topic", null, Encoding.Unicode.GetBytes("Hello World!"));
        
        // Assert
        _channelMock.Verify(c => c.BasicAck(It.IsAny<ulong>(), true));
    }

    [TestMethod]
    public void BasicReciever_WhenStartingRecievingMessageIncorrectly_ShouldSendNack()
    {
        // Arrange
        EventingBasicConsumer? consumer = null;

        _channelMock
            .Setup(c => c.BasicConsume(QueueName, false, It.IsAny<string>(), false, false, null, It.IsAny<EventingBasicConsumer>()))
            .Callback<string, bool, string, bool, bool, IDictionary<string, object>, IBasicConsumer>((_, _, _, _, _, _, e) => consumer = (EventingBasicConsumer)e);

        // Act
        _reciever.SetUpQueue(new []{ "Wrappit.Topic" });
        _reciever.StartRecieving(_ => throw new WrappitException(""));
        
        consumer?.HandleBasicDeliver("", 0, false, ExchangeName, "Wrappit.Topic", null, Encoding.Unicode.GetBytes("Hello World!"));
        
        // Assert
        _channelMock.Verify(c => c.BasicAck(It.IsAny<ulong>(), true), Times.Never);
        _channelMock.Verify(c => c.BasicNack(It.IsAny<ulong>(), false, true));
    }

    [TestMethod]
    public void BasicReciever_WhenDisposingButNoChannel_ShouldNotDisposeNonExistingChannel()
    {
        // Arrange & Act
        _reciever.Dispose();

        // Assert
        _channelMock.Verify(c => c.Dispose(), Times.Never);
    }

    [TestMethod]
    public void BasicReciever_WhenDisposingCreatedChannel_ShouldDisposeChannel()
    {
        // Arrange & Act
        _reciever.SetUpQueue(new []{ "Wrappit.Topic" });
        _reciever.StartRecieving(_ => {});
        _reciever.Dispose();

        // Assert
        // The .SetUpQueue also uses a Dispose method, which is why we use '2'.
        _channelMock.Verify(c => c.Dispose(), Times.Exactly(2));
    }

    [TestMethod]
    public void BasicReciever_WhenDependencyInjectionFails_ShouldFail()
    {
        // Arrange
        EventingBasicConsumer? consumer = null;

        _channelMock
            .Setup(c => c.BasicConsume(QueueName, false, It.IsAny<string>(), false, false, null, It.IsAny<EventingBasicConsumer>()))
            .Callback<string, bool, string, bool, bool, IDictionary<string, object>, IBasicConsumer>((_, _, _, _, _, _, e) => consumer = (EventingBasicConsumer)e);

        // Act
        _reciever.SetUpQueue(new []{ "Wrappit.Topic" });
        _reciever.StartRecieving(_ => throw new InvalidOperationException());
        
        void ShouldFail() => consumer?.HandleBasicDeliver("", 0, false, ExchangeName, "Wrappit.Topic", null, Encoding.Unicode.GetBytes("Hello World!"));
        
        // Assert
        var exception = Assert.ThrowsException<WrappitException>(ShouldFail);
        Assert.AreEqual("Dependency injection failed.", exception.Message);
    }
}
