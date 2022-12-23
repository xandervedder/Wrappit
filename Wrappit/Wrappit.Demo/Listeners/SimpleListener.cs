using Wrappit.Attributes;
using Wrappit.Demo.Controllers;

namespace Wrappit.Demo.Listeners;

[EventListener]
public class SimpleListener
{
    [Handle("Demo.Topic")]
    public void Handle(ExampleEvent @event)
    {
        Console.WriteLine(@event.ExampleProperty);
        Console.WriteLine(@event.CorrelationId);
        Console.WriteLine(@event.DateTime);
    }
}
