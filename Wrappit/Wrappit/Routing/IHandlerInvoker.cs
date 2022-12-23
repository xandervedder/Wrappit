using Wrappit.Messaging;

namespace Wrappit.Routing;

internal interface IHandlerInvoker
{
    public string Topic { get; }
    public void Dispatch(EventMessage message);
}
