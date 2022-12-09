using Wrappit.Attributes;

namespace Wrappit.Test.Routing.Mocks;

[EventListener]
public class AsyncMockListener
{
    private readonly IMockService _service;

    public AsyncMockListener(IMockService service)
    {
        _service = service;
    }

    [Handle("Wrappit.Topic")]
    public async Task Handle(MockEvent mockEvent)
    {
        await _service.DoMethodAsync(mockEvent);
    }
}