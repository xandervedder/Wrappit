using Wrappit.Attributes;
using Wrappit.Demo.Controllers;

namespace Wrappit.Demo;

[EventListener]
public class DependencyInjectionListener
{
    private readonly IExample _example;

    public DependencyInjectionListener(IExample example)
    {
        _example = example;
    }

    [Handle("Demo.Topic")]
    public void Handle(ExampleEvent @event)
    {
        _example.DoStuff();
    }
}

public interface IExample
{
    public void DoStuff();
}

public class Example : IExample
{
    public void DoStuff()
    {
        Console.WriteLine("Doing stuff!");
    }
}
