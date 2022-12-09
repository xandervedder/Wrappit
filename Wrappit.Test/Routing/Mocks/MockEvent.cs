using Wrappit.Messaging;

namespace Wrappit.Test.Routing.Mocks;

public class MockEvent : DomainEvent
{
    public string Name { get; }

    public MockEvent(string name)
    {
        Name = name;
    }
}
