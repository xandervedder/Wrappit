using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Wrappit.Messaging;
using Wrappit.Routing;
using Wrappit.Test.Routing.Mocks;

namespace Wrappit.Test.Routing;

[TestClass]
public class HandlerInvokerTest
{
    private Mock<IMockService> _service = null!;
    private ServiceProvider _provider = null!;
    
    [TestInitialize]
    public void Setup()
    {
        _service = new Mock<IMockService>();
        _service.Setup(s => s.DoMethodAsync(It.IsAny<MockEvent>())).Returns(new Task(() => { }));
        _provider = new ServiceCollection()
            .AddSingleton(_ => _service.Object)
            .BuildServiceProvider();
    }
    [TestMethod]
    public void HandlerInvokerTest_ShouldInvokeCorrectly()
    {
        // Arrange
        var eventListenerType = typeof(MockListener);
        var method = eventListenerType.GetMethod("Handle")!;
        var parameterType = typeof(MockEvent);
        var invoker = new HandlerInvoker(eventListenerType, method, parameterType, "Wrappit.Topic", _provider, new NullLogger<HandlerInvoker>());
        var mockEvent = new MockEvent("Evert 't Reve");
        var eventMessage = new EventMessage("Wrappit.Topic", JsonConvert.SerializeObject(mockEvent));
            
        // Act
        invoker.Dispatch(eventMessage);

        // Assert
        _service.Verify(s => s.DoMethod(It.Is<MockEvent>(o => o.Name == "Evert 't Reve")));
    }

    [TestMethod]
    public void HandlerInvokerTest_ShouldInvokeAsyncCorrectly()
    {
        // Arrange
        var eventListenerType = typeof(AsyncMockListener);
        var method = eventListenerType.GetMethod("Handle")!;
        var parameterType = typeof(MockEvent);
        var invoker = new HandlerInvoker(eventListenerType, method, parameterType, "Wrappit.Topic", _provider, new NullLogger<HandlerInvoker>());
        var mockEvent = new MockEvent("Evert 't Reve");
        var eventMessage = new EventMessage("Wrappit.Topic", JsonConvert.SerializeObject(mockEvent));
            
        // Act
        invoker.Dispatch(eventMessage);

        // Assert
        Thread.Sleep(100);
        _service.Verify(s => s.DoMethodAsync(It.IsAny<MockEvent>()));
    }

    [TestMethod]
    public void HandlerInvokerTest_WhenArgumentsAreNotCorrect_ShouldFail()
    {
        // Arrange
        var eventListenerType = typeof(BadArgumentsInConstructorMockListener);
        var method = eventListenerType.GetMethod("Handle")!;
        var parameterType = typeof(MockEvent);
        var invoker = new HandlerInvoker(eventListenerType, method, parameterType, "Wrappit.Topic", _provider, new NullLogger<HandlerInvoker>());
        var mockEvent = new MockEvent("Evert 't Reve");
        var eventMessage = new EventMessage("Wrappit.Topic", JsonConvert.SerializeObject(mockEvent));
        
        // Act
        void ShouldFail() => invoker.Dispatch(eventMessage);

        // Assert
        Assert.ThrowsException<InvalidOperationException>(ShouldFail);
    }
}
