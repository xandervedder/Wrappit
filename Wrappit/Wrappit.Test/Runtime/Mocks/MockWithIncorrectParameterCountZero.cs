using Wrappit.Attributes;

namespace Wrappit.Test.Runtime.Mocks;

[EventListener]
public class MockWithIncorrectParameterCountZero
{
    [Handle("Wrappit.Topic")]
    public void Handle()
    {
        
    }
}