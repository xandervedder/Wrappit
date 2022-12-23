using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Wrappit.Messaging;
using Wrappit.Routing;
using Wrappit.Services;

namespace Wrappit.Test.Services;

[TestClass]
public class WrappitHostedServiceTest
{
    private Mock<IWrappitRouter> _routerMock = null!;
    private Mock<IBasicReciever> _recieverMock = null!;
    private IHostedService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _routerMock = new Mock<IWrappitRouter>();
        _recieverMock = new Mock<IBasicReciever>();
        _recieverMock.Setup(r => r.StartRecieving(It.IsAny<Action<EventMessage>>()))
            .Callback<Action<EventMessage>>(c => c(It.IsAny<EventMessage>()));
        _sut = new WrappitHostedService(_recieverMock.Object, _routerMock.Object, new NullLogger<WrappitHostedService>());
    }

    [TestMethod]
    public async Task WrappitHostedService_StartAsync_ShouldCallRouteMethod()
    {
        // Arrange
        _routerMock.Setup(r => r.Topics)
            .Returns(new List<string> { "Topic 1", "Topic 2" });
        
        // Act
        await _sut.StartAsync(new CancellationToken());

        // Assert
        _routerMock.Verify(r => r.Route(It.IsAny<EventMessage>()));
    }

    [TestMethod]
    public async Task WrappitHostedService_Router_ShouldBeCalledWithCorrectTopics()
    {
        // Arrange
        _routerMock.Setup(r => r.Topics)
            .Returns(new List<string> { "Topic 1", "Topic 2" });
        
        // Act
        await _sut.StartAsync(new CancellationToken());

        // Assert
        _recieverMock.Verify(r => r.SetUpQueue(It.Is<IEnumerable<string>>(t => t.All(s => s.Equals("Topic 1") || s.Equals("Topic 2")))));
    }

    [TestMethod]
    public async Task WrappitHostedService_StartAsync_ShouldSetUpQueueAndStartListing()
    {
        // Arrange
        _routerMock.Setup(r => r.Topics)
            .Returns(new List<string> { "Topic 1", "Topic 2" });
        
        // Act
        await _sut.StartAsync(new CancellationToken());

        // Assert
        _recieverMock.Verify(r => r.SetUpQueue(It.IsAny<IEnumerable<string>>()));
        _recieverMock.Verify(r => r.StartRecieving(It.IsAny<Action<EventMessage>>()));
    }

    [TestMethod]
    public async Task WrappitHostedService_StopAsync_ShouldDisposeReciever()
    {
        // Act
        await _sut.StopAsync(new CancellationToken());

        // Assert
        _recieverMock.Verify(r => r.Dispose());
    }

    [TestMethod]
    public async Task WrappitHostedService_StartAsync_ShouldNotListingWhenNoTopicsAvailableAtRuntime()
    {
        // Arrange
        _routerMock.Setup(r => r.Topics).Returns(new List<string>());
        
        // Act
        await _sut.StartAsync(new CancellationToken());

        // Assert
        _recieverMock.Verify(r => r.StartRecieving(It.IsAny<Action<EventMessage>>()), Times.Never);
        _recieverMock.Verify(r => r.SetUpQueue(It.IsAny<IEnumerable<string>>()), Times.Never);
    }
}
