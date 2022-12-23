using Wrappit.Attributes;

namespace Wrappit.Test.Routing.Mocks;

[EventListener]
public class MockListener
{
    private readonly IMockService _service;

    public MockListener(IMockService service)
    {
        _service = service;
    }

    [Handle("Wrappit.Topic")]
    public void Handle(MockEvent message)
    {
        _service.DoMethod(message);
    }
}
