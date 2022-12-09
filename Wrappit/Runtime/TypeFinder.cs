using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Wrappit.Runtime;

internal class TypeFinder : ITypeFinder
{
    public IEnumerable<Type> FindAllTypes()
    {
        var assemblies = GetRefencingAssemblies();
        return assemblies.SelectMany(a => a.GetTypes());
    }
    
    private IEnumerable<Assembly> GetRefencingAssemblies()
    {
        var thisAssemblyName = GetType().Assembly.FullName!;
 
        var result = from library in DependencyContext.Default!.RuntimeLibraries
            where library.Dependencies.Any(d => thisAssemblyName.StartsWith(d.Name))
            select Assembly.Load(new AssemblyName(library.Name));
        return result;
    }
}
