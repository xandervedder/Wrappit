using Wrappit.Attributes;
using Wrappit.Test.Routing.Mocks;

namespace Wrappit.Test.Runtime.Mocks;

[EventListener]
public class MockWithMultipleHandlers
{
    private readonly IMockService _service;

    public MockWithMultipleHandlers(IMockService service)
    {
        _service = service;
    }

    [Handle("Wrappit.A")]
    public void HandleA(MockEvent @event)
    {
        _service.DoMethod(@event);
    }

    [Handle("Wrappit.B")]
    public void HandleB(MockEvent @event)
    {
        _service.DoMethod(@event);
    }
}