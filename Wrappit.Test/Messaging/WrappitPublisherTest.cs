using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using Wrappit.Configuration;
using Wrappit.Messaging;
using Wrappit.Test.Messaging.Mocks;

namespace Wrappit.Test.Messaging;

[TestClass]
public class WrappitPublisherTest
{
    private const string Topic = "Wrappit.Topic";

    private Mock<IBasicSender> _senderMock = null!;
    
    [TestInitialize]
    public void Setup()
    {
        _senderMock = new Mock<IBasicSender>();
    }

    [TestMethod]
    public void WrappitPublisher_WhenUsingContextConstructor_ShouldCreateBasicSender()
    {
        // Arrange
        var channelMock = new Mock<IModel>();
        var contextMock = new Mock<IWrappitContext>();
        contextMock.Setup(c => c.CreateChannel()).Returns(channelMock.Object);
        var publisher = new WrappitPublisher(contextMock.Object, new NullLogger<BasicSender>(), new NullLogger<WrappitPublisher>());
        var evt = new DerivedDomainEventMock { Name = "Evert 't Reve" };

        // Act
        publisher.Publish(Topic, evt);

        // Assert
        contextMock.Verify(c => c.CreateChannel());
    }
    
    [TestMethod]
    public void WrappitPublisher_WhenSendingMessage_ShouldSendMessageCorrectly()
    {
        // Arrange
        var publisher = new WrappitPublisher(_senderMock.Object, new NullLogger<WrappitPublisher>());
        var evt = new DerivedDomainEventMock { Name = "Evert 't Reve" };

        // Act
        publisher.Publish(Topic, evt);

        // Assert
        _senderMock.Verify(s => s.Send(It.Is<EventMessage>(e => e.Body.Contains("\"Name\":\"Evert 't Reve\""))));
    }    
}
