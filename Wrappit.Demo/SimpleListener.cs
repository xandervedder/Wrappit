using Wrappit.Attributes;
using Wrappit.Demo.Controllers;

namespace Wrappit.Demo;

[EventListener]
public class SimpleListener
{
    [Handle("Demo.Topic")]
    public void Handle(ExampleEvent e)
    {
        Console.WriteLine(e.ExampleProperty);
        Console.WriteLine(e.CorrelationId);
        Console.WriteLine(e.DateTime);
    }
}
