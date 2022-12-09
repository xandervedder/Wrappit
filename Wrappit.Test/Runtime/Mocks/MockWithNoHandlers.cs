using Wrappit.Attributes;
using Wrappit.Test.Routing.Mocks;

namespace Wrappit.Test.Runtime.Mocks;

[EventListener]
public class MockWithNoHandlers
{
    public void Handle(MockEvent @event)
    {
        // Method intentionally left empty.
    }
}
