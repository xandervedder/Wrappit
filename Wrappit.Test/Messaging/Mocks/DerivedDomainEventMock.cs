using Wrappit.Messaging;

namespace Wrappit.Test.Messaging.Mocks;

public class DerivedDomainEventMock : DomainEvent
{
    public string Name { get; set; } = null!;
}
