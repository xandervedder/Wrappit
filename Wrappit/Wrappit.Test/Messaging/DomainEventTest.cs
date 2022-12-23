using Wrappit.Messaging;

namespace Wrappit.Test.Messaging;

[TestClass]
public class DomainEventTest
{
    [TestMethod]
    public void DomainEvent_ShouldContainDefaultProperties()
    {
        // Arrange & Act
        var domainEvent = new DomainEvent();

        // Assert
        Assert.IsTrue(DateTime.UtcNow > domainEvent.DateTime);
        Assert.AreEqual(36, domainEvent.CorrelationId.ToString().Length);
    }
}
