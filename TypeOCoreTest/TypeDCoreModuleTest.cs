using TypeDCore;
using System.Reflection;

namespace TypeOCoreTest;

public class TypeDCoreModuleTest
{
    private TypeDCoreInitializer CreateInitializer()
    {
        var initializer = new TypeDCoreInitializer();

        Type t = initializer.GetType();
        t.InvokeMember("Hooks", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, initializer, new object[] { new HookModelMock() });
        t.InvokeMember("Resources", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, initializer, new object[] { new ResourceModelMock() });

        return initializer;
    }

    [Fact]
    public void StartTest()
    {
        //var initializer = CreateInitializer();
        //Assert.NotNull(initializer);
        //initializer.Initializer(null);
        //initializer.Uninitializer();
    }
}