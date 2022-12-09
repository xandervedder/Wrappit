namespace Wrappit.Messaging;

public class DomainEvent
{
    public DateTime DateTime { get; }
    public Guid CorrelationId { get; set; }

    public DomainEvent()
    {
        DateTime = DateTime.UtcNow;
        CorrelationId = Guid.NewGuid();
    }
}
