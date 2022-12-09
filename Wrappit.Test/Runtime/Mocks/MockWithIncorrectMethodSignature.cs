using Wrappit.Attributes;

namespace Wrappit.Test.Runtime.Mocks;

[EventListener]
public class MockWithIncorrectMethodSignature
{
    [Handle("topic")]
    public void Handle(MockWithIncorrectMethodSignature one)
    {
        // Method intentionally left empty.
    }
}
