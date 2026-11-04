using System.Reflection;
using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Attributes;
using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;
using TypeOEngine.Typedeaf.Core.Interfaces;

namespace TypeOCoreTest;

public class TypeOPropertyAttributeTests
{
    [Fact]
    public void ExistingConstructor_DefaultsToGeneralAndPreservesMetadata()
    {
        var attribute = new TypeOPropertyAttribute("Description", 42);

        Assert.Equal("General", attribute.Group);
        Assert.Equal("Description", attribute.Description);
        Assert.Equal(42, attribute.DefaultValue);
    }

    [Fact]
    public void Group_CanBeSetByConstructorOrNamedArgument()
    {
        Assert.Equal("Transform", new TypeOPropertyAttribute("Position", null, "Transform").Group);
        Assert.Equal("Rendering", new TypeOPropertyAttribute { Group = "Rendering" }.Group);
    }

    [Theory]
    [InlineData(typeof(TypeOObject), "ID", "General")]
    [InlineData(typeof(TypeOObject), "Name", "General")]
    [InlineData(typeof(Scene), "Pause", "Behavior")]
    [InlineData(typeof(Scene), "Hide", "Rendering")]
    [InlineData(typeof(IUpdatable), "Pause", "Behavior")]
    [InlineData(typeof(IDrawable), "Hidden", "Rendering")]
    [InlineData(typeof(IDrawable), "DrawOrder", "Rendering")]
    public void BuiltInProperties_HaveRelevantGroups(Type type, string name, string group)
    {
        var attribute = type.GetProperty(name).GetCustomAttribute<TypeOPropertyAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal(group, attribute.Group);
    }
}
