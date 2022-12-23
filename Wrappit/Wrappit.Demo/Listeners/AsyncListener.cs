using Wrappit.Attributes;
using Wrappit.Demo.Controllers;

namespace Wrappit.Demo.Listeners;

[EventListener]
public class AsyncListener
{
    [Handle("Demo.Async")]
    public async Task HandleAsync(ExampleEvent @event)
    {
        await Task.Delay(2000);
        Console.WriteLine(@event.ExampleProperty);
    }
}
