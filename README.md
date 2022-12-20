# Wrappit

<p align="center" width="100%">
    <img width="33%" src="logo.png"> 
</p>

Wrappit is a simple wrapper around RabbitMQ which makes it easier to both send and recieve messages from RabbitMQ.

**Note: Currently, Wrappit only supports Topical exchanges with no plans on supporting other types of exchanges.**

## Usage

### Configuration

To use Wrappit, use the following method in the services:

```csharp

builder.Services.AddWrappit(new WrappitOptions
{
	HostName = "...",
	Port = 1234,
	UserName = "...",
	Password = "...",
	ExchangeName = "...",
	QueueName = "...",
	// Optionally add a delivery limit
	DeliveryLimit = 5,
});

```

These options can be ommitted if the following environment variables have been set:
 * `Wrappit_HostName`
 * `Wrappit_Port`
 * `Wrappit_UserName`
 * `Wrappit_Password`
 * `Wrappit_ExchangeName`
 * `Wrappit_QueueName`
 * `Wrappit_DeliveryLimit`
   * This one is optional, the default is `10`. This delivery limit is used when a handle method fails, this prevents infinite requeuing (see the related [issue](https://github.com/xandervedder/Wrappit/issues/1)).

Once all of the environment variables have been set, the following can be done:

```csharp
builder.Services.AddWrappit();
``` 

### Listening to events

Listening to a certain event is easy, simply add the `[EventListener]` and `[Handle("Topic")]` Attributes.
The handle method requires exactly one parameter, which is the Event. This event needs to inherit from the class `DomainEvent`:

```csharp
using Wrappit.Messaging;

public class ExampleEvent : DomainEvent 
{
    public string ExampleProperty { get; set; }
}
```

By default, the `DomainEvent` class has a `CorrelationId` and a `DateTime` property (for debugging messages).  

```csharp
using Wrappit.Attributes;

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
```

It is also possible to use multiple Handlers in the same event listener class. It is also possible to have multiple handlers listen to the same topic, however, if one of these handlers fails the message will be requeued.

To allow complex logic to take place, it is also possible to use dependency injection in the EventListener class:

```csharp
using Wrappit.Attributes;

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
```

### Publishing an event

Publishing an event is even easier, simply add the `IWrappitPublisher` to your constructor. Dependency injection will take care of the rest:

```csharp
using Wrappit.Messaging;

public class MessageController
{
    private readonly IWrappitPublisher _publisher;

    public MessageController(IWrappitPublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost]
    public void Send(string message)
    {
        _publisher.Publish("Demo.Topic", new ExampleEvent { ExampleProperty = message });
    }
}
```

### Bugs?

Please make an issue, I will take a look at it (if time allows).
