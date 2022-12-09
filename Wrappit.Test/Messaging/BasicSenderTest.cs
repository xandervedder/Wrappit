using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using Wrappit.Configuration;
using Wrappit.Messaging;

namespace Wrappit.Test.Messaging;

[TestClass]
public class BasicSenderTest
{
    private const string Exchange = "MyExchange";
    private const string Topic = "MyTopic";
    private const string Body = "Hello World!";
    
    private Mock<IWrappitContext> _contextMock = null!;
    private Mock<IModel> _channelMock = null!;
    
    [TestInitialize]
    public void Setup()
    {
        _channelMock = new Mock<IModel>();
        _contextMock = new Mock<IWrappitContext>();
        _contextMock.Setup(c => c.ExchangeName).Returns(Exchange);
        _contextMock.Setup(c => c.CreateChannel()).Returns(_channelMock.Object);
    }
    
    [TestMethod]
    public void BasicSender_WhenSendingAMessage_ShouldCreateChannelOnce()
    {
        // Arrange
        var reciever = new BasicSender(_contextMock.Object, new NullLogger<BasicSender>());
        var message = new EventMessage(Topic, Body);

        // Act
        reciever.Send(message);

        // Assert
        _contextMock.Verify(c => c.CreateChannel());
    }

    [TestMethod]
    public void BasicSender_WhenSendingAMessage_ShouldSendCorrectMessage()
    {
        // Arrange
        var reciever = new BasicSender(_contextMock.Object, new NullLogger<BasicSender>());
        var message = new EventMessage(Topic, Body);

        // Act
        reciever.Send(message);

        // Assert
        _channelMock.Verify(c => c.BasicPublish(Exchange, Topic, true, null, 
            It.Is<ReadOnlyMemory<byte>>(b => Encoding.Unicode.GetString(b.ToArray()) == Body)));
    }

    [TestMethod]
    public void BasicSender_WhenSendingAMessage_ShouldNotEqualToDifferingMessage()
    {
        // Arrange
        var reciever = new BasicSender(_contextMock.Object, new NullLogger<BasicSender>());
        var message = new EventMessage(Topic, Body);

        // Act
        reciever.Send(message);

        // Assert
        _channelMock.Verify(c => c.BasicPublish(Exchange, Topic, true, null, 
            It.Is<ReadOnlyMemory<byte>>(b => Encoding.Unicode.GetString(b.ToArray()) == "Text that should not match")), Times.Never);
    }
}
