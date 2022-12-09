namespace Wrappit.Messaging;

internal interface IBasicReciever : IDisposable
{
    public void SetUpQueue(IEnumerable<string> topics);
    public void StartRecieving(Action<EventMessage> handler);
}
