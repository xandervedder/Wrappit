using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Wrappit.Exceptions;
using Wrappit.Messaging;
using Wrappit.Routing;
using Wrappit.Runtime;
using Wrappit.Test.Routing.Mocks;
using Wrappit.Test.Runtime.Mocks;

namespace Wrappit.Test.Runtime;

[TestClass]
public class RuntimeBuilderTest
{
    private Mock<IMockService> _serviceMock = null!;
    private IServiceProvider _serviceProvider = null!;
    private MockEvent _mockEvent = null!;
    private RuntimeBuilder _builder = null!;

    [TestInitialize]
    public void Setup()
    {
        _serviceMock = new Mock<IMockService>();
        _serviceProvider = new ServiceCollection()
            .AddTransient(_ => _serviceMock.Object)
            .BuildServiceProvider();
        _mockEvent = new MockEvent("Wrappit");
        _builder = new RuntimeBuilder(new TypeFinder(), _serviceProvider, new NullLogger<RuntimeBuilder>(), new NullLogger<HandlerInvoker>(), new NullLogger<WrappitRouter>());
    }
    
    [TestMethod]
    public void RuntimeBuilder_WithParameterCountOfTwo_ShouldFail()
    {
        // Act
        void ShouldFail() => _builder.RegisterEventListener(typeof(MockWithIncorrectParameterCountMoreThanOne));

        // Assert
        var exception = Assert.ThrowsException<WrappitException>(ShouldFail);
        Assert.AreEqual("Method signature of Handle is invalid, please provide exactly one parameter.", exception.Message);
    }

    [TestMethod]
    public void RuntimeBuilder_WithParameterCountOfZero_ShouldFail()
    {
        // Act
        void ShouldFail() => _builder.RegisterEventListener(typeof(MockWithIncorrectParameterCountZero));

        // Assert
        var exception = Assert.ThrowsException<WrappitException>(ShouldFail);
        Assert.AreEqual("Method signature of Handle is invalid, please provide exactly one parameter.", exception.Message);
    }

    [TestMethod]
    public void RuntimeBuilder_WithIncorrectMethodSignature_ShouldFail()
    {
        // Act
        void ShouldFail() => _builder.RegisterEventListener(typeof(MockWithIncorrectMethodSignature));

        // Assert
        var exception = Assert.ThrowsException<WrappitException>(ShouldFail);
        Assert.AreEqual("Parameter type should derive from DomainEvent.", exception.Message);
    }

    [TestMethod]
    public void RuntimeBuilder_WithMultipleHandlers_ShouldInstantiateBoth()
    {
        // Arrange
        var message1 = new EventMessage("Wrappit.A", JsonConvert.SerializeObject(_mockEvent));
        var message2 = new EventMessage("Wrappit.B", JsonConvert.SerializeObject(_mockEvent));

        // Act
        _builder.RegisterEventListener(typeof(MockWithMultipleHandlers));
        var router = _builder.Build();
        router.Route(message1);
        router.Route(message2);

        // Assert
        _serviceMock.Verify(s => s.DoMethod(It.IsAny<MockEvent>()), Times.Exactly(2));
    }

    [TestMethod]
    public void RuntimeBuilder_WithNoHandlers_ShouldInstantiateNothing()
    {
        // Arrange
        var message = new EventMessage("Wrappit.A", JsonConvert.SerializeObject(_mockEvent));

        // Act
        _builder.RegisterEventListener(typeof(MockWithNoHandlers));
        var router = _builder.Build();
        router.Route(message);

        // Assert
        _serviceMock.Verify(s => s.DoMethod(It.IsAny<MockEvent>()), Times.Never);
    }
    
    [TestMethod]
    public void RuntimeBuilder_ShouldDiscoverListenersCorrectly()
    {
        // Arrange
        var typeFinderMock = new Mock<ITypeFinder>();
        typeFinderMock.Setup(t => t.FindAllTypes()).Returns(new List<Type> { typeof(MockWithMultipleHandlers), typeof(MockWithOneHandler) });
        var builder = new RuntimeBuilder(typeFinderMock.Object, _serviceProvider, new NullLogger<RuntimeBuilder>(), new NullLogger<HandlerInvoker>(), new NullLogger<WrappitRouter>());
        
        var message1 = new EventMessage("Wrappit.A", JsonConvert.SerializeObject(_mockEvent));
        var message2 = new EventMessage("Wrappit.B", JsonConvert.SerializeObject(_mockEvent));
        var message3 = new EventMessage("Wrappit.Topic", JsonConvert.SerializeObject(_mockEvent));

        // Act
        var router = builder.DiscoverAndRegisterAllEventListeners().Build();
        router.Route(message1);
        router.Route(message2);
        router.Route(message3);

        // Assert
        Assert.AreEqual(3, router.Topics.Count());
        
        _serviceMock.Verify(s => s.DoMethod(It.IsAny<MockEvent>()), Times.Exactly(3));
    }
}