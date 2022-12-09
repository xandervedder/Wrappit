using Wrappit.Attributes;
using Wrappit.Test.Routing.Mocks;

namespace Wrappit.Test.Runtime.Mocks;

[EventListener]
public class MockWithIncorrectParameterCountMoreThanOne
{
    [Handle("Wrappit.Topic")]
    public void Handle(MockEvent one, MockEvent two)
    {
        // Method intentionally left empty.
    }
}
