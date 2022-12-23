using Microsoft.Extensions.Logging.Abstractions;
using Wrappit.Messaging;
using Wrappit.Routing;

namespace Wrappit.Test.Routing;

[TestClass]
public class WrappitRouterTest
{
    private Mock<IHandlerInvoker> _handlerMock1 = null!;
    private Mock<IHandlerInvoker> _handlerMock2 = null!;
    private Mock<IHandlerInvoker> _handlerMock3 = null!;
    private WrappitRouter _router = null!;

    [TestInitialize]
    public void Setup()
    {
        _handlerMock1 = new Mock<IHandlerInvoker>();
        _handlerMock2 = new Mock<IHandlerInvoker>();
        _handlerMock3 = new Mock<IHandlerInvoker>();
        _handlerMock1.Setup(h => h.Topic).Returns("Wrappit.Topic1");
        _handlerMock2.Setup(h => h.Topic).Returns("Wrappit.Topic2");
        _handlerMock3.Setup(h => h.Topic).Returns("Wrappit.Topic3");
        _router = new WrappitRouter(
            new[] { _handlerMock1.Object, _handlerMock2.Object, _handlerMock3.Object },
            new NullLogger<WrappitRouter>());
    }
    
    [TestMethod]
    public void WrappitRouter_WhenCreatingRouter_ShouldHaveThreeHandlers()
    {
        Assert.AreEqual(3, _router.Handlers.Count());
    }

    [TestMethod]
    public void WrappitRouter_WhenCreatingRouter_ShouldInitializeTopics()
    {
        Assert.IsTrue(_router.Topics
            .All(t => t.Equals("Wrappit.Topic1") || t.Equals("Wrappit.Topic2") || t.Equals("Wrappit.Topic3")));
    }

    [TestMethod]
    public void WrappitRouter_WhenRouting_ShouldDispatchToHandlerWithSameTopic()
    {
        // Arrange
        var message = new EventMessage("Wrappit.Topic1", "Hello World");
        
        // Act
        _router.Route(message);

        // Assert
        _handlerMock1.Verify(h => h.Dispatch(message));
    }

    [TestMethod]
    public void WrappitRouter_WhenRouting_ShouldSendMessageCorrectly()
    {
        // Arrange
        var message = new EventMessage("Wrappit.Topic1", "Hello World");

        EventMessage recievedMessage = null!;
        _handlerMock1.Setup(h => h.Dispatch(message))
            .Callback<EventMessage>(m => recievedMessage = m);

        // Act
        _router.Route(message);

        // Assert
        Assert.AreEqual("Wrappit.Topic1", recievedMessage.Topic);
        Assert.AreEqual("Hello World", recievedMessage.Body);
    }

    [TestMethod]
    public void WrappitRouter_WhenRouting_ShouldDispatchToMultipleHandlersWithTheSameTopic()
    {
        // Arrange
        var message = new EventMessage("Wrappit.Topic1", "Hello World");
        var mockWithSameTopic = new Mock<IHandlerInvoker>();
        mockWithSameTopic.Setup(m => m.Topic).Returns("Wrappit.Topic1");

        var router = new WrappitRouter(
            new[] { _handlerMock1.Object, mockWithSameTopic.Object },
            new NullLogger<WrappitRouter>());
        
        // Act
        router.Route(message);

        // Assert
        _handlerMock1.Verify(h => h.Dispatch(message));
        mockWithSameTopic.Verify(m => m.Dispatch(message));
    }
}
