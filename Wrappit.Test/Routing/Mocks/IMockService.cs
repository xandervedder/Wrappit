namespace Wrappit.Test.Routing.Mocks;

public interface IMockService
{
    void DoMethod(MockEvent evt);
    Task DoMethodAsync(MockEvent evt);
}