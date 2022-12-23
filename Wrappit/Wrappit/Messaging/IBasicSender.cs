namespace Wrappit.Messaging;

internal interface IBasicSender
{
    public void Send(EventMessage message);
}
