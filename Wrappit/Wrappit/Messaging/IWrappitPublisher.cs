namespace Wrappit.Messaging;

public interface IWrappitPublisher
{
    public void Publish<T>(string topic, T evt) where T : DomainEvent;
}
