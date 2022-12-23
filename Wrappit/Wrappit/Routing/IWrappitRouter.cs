using Wrappit.Messaging;

namespace Wrappit.Routing;

internal interface IWrappitRouter
{
    public IEnumerable<string> Topics { get; }
    public void Route(EventMessage eventMessage);
}
