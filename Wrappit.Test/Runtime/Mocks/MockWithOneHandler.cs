using Wrappit.Attributes;
using Wrappit.Test.Routing.Mocks;

namespace Wrappit.Test.Runtime.Mocks;

[EventListener]
public class MockWithOneHandler
{
    private readonly IMockService _service;

    public MockWithOneHandler(IMockService service)
    {
        _service = service;
    }

    [Handle("Wrappit.Topic")]
    public void Handle(MockEvent @event)
    {
        _service.DoMethod(@event);
    }
}
