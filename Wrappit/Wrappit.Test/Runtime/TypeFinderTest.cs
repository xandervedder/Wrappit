using Wrappit.Runtime;

namespace Wrappit.Test.Runtime;

[TestClass]
public class TypeFinderTest
{
    [TestMethod]
    public void TypeFinder_ShouldFindAllTypes()
    {
        // Arrange
        var typeFinder = new TypeFinder();

        // Act
        var types = typeFinder.FindAllTypes();

        // Assert
        Assert.IsTrue(types.Any(t => t.FullName == "Wrappit.Test.Runtime.TypeFinderTest"));
    }    
}
