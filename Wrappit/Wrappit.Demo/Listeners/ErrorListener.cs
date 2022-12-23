using Wrappit.Attributes;
using Wrappit.Demo.Controllers;

namespace Wrappit.Demo.Listeners;

[EventListener]
public class ErrorListener
{
    [Handle("Demo.Error")]
    public void Handle(ExampleEvent @event)
    {
        throw new NotImplementedException();
    }
}
