using Wrappit.Routing;

namespace Wrappit.Runtime;

internal interface IRuntimeBuilder
{
    public IRuntimeBuilder RegisterEventListener(Type eventListenerType);
    public IRuntimeBuilder DiscoverAndRegisterAllEventListeners();
    public IWrappitRouter Build();
}
