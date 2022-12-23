using Wrappit.Attributes;
using Wrappit.Routing;

namespace Wrappit.Test.Routing.Mocks;

[EventListener]
internal class BadArgumentsInConstructorMockListener
{
    private readonly IWrappitRouter _router;
    private readonly IHandlerInvoker _invoker;
    
    public BadArgumentsInConstructorMockListener(IWrappitRouter router, IHandlerInvoker invoker)
    {
        _router = router;
        _invoker = invoker;
    }
    
    [Handle("Wrappit.Topic")]
    public void Handle(MockEvent evt)
    {
    }
}