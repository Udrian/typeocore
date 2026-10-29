using System.Reflection;
using TypeD.Code;
using TypeD.Helpers;
using TypeD.Models.Data;
using TypeDCore.Code.Drawable;
using TypeDCore.Code.Entity;
using TypeDCore.Components;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeOCoreTest;

/// <summary>
/// Pins down how the root engine type of a component is used, so that the way it is determined can be changed safely.
/// </summary>
public class ComponentBaseTypeTests
{
    private const string EntityName = "TypeOEngine.Typedeaf.Core.Entities.Entity";
    private const string DrawableName = "TypeOEngine.Typedeaf.Core.Entities.Drawables.Drawable";
    private const string SceneName = "TypeOEngine.Typedeaf.Core.Scene";
    private const string GameName = "TypeOEngine.Typedeaf.Core.Game";

    // The only place that reads how the root type is exposed, switch it when the API changes.
    private static string RootType(Component component) => component.RootBaseComponent?.FullName;

    public static IEnumerable<object[]> BaseDefinitions()
    {
        yield return new object[] { "Entity", (Func<Component>)CoreComponent.EntityComponent, EntityName };
        yield return new object[] { "Drawable", (Func<Component>)CoreComponent.DrawableComponent, DrawableName };
        yield return new object[] { "Scene", (Func<Component>)CoreComponent.SceneComponent, SceneName };
        yield return new object[] { "Game", (Func<Component>)CoreComponent.GameComponent, GameName };
    }

    [Theory]
    [MemberData(nameof(BaseDefinitions))]
    public void BaseDefinition_HasExpectedRootType(string _, Func<Component> create, string expected)
    {
        Assert.Equal(expected, RootType(create()));
    }

    [Fact]
    public void DerivedComponent_HasRootTypeOfItsBase()
    {
        var entity = CoreComponent.EntityComponent();
        var player = Derive("Player", entity);
        var boss = Derive("Boss", player);

        Assert.Equal(EntityName, RootType(player));
        Assert.Equal(EntityName, RootType(boss));
    }

    [Fact]
    public void ComponentWithoutBase_HasNoMatchingRootType()
    {
        var component = new Component { ClassName = "Loose", Namespace = "Game" };

        Assert.NotEqual(EntityName, RootType(component));
        Assert.NotEqual(DrawableName, RootType(component));
    }

    [Theory]
    [InlineData("Entity", true, true)]
    [InlineData("Scene", true, false)]
    [InlineData("Drawable", false, true)]
    public void ChildFilter_AcceptsExpectedRootTypes(string parentName, bool acceptsEntity, bool acceptsDrawable)
    {
        var parent = BaseDefinitions().First(d => (string)d[0] == parentName)[1] as Func<Component>;
        var filter = new FilterHelper();
        parent().Template.ChildrenFilter(filter);

        Assert.Equal(acceptsEntity, filter.Filters.Contains($"{EntityName};"));
        Assert.Equal(acceptsDrawable, filter.Filters.Contains($"{DrawableName};"));
    }

    [Fact]
    public void ChildFilter_DecidesByRootTypeOfDerivedChildren()
    {
        var parent = CoreComponent.EntityComponent();
        var filter = new FilterHelper();
        parent.Template.ChildrenFilter(filter);

        var derivedEntity = Derive("Player", CoreComponent.EntityComponent());
        var derivedDrawable = Derive("Sprite", CoreComponent.DrawableComponent());
        var derivedScene = Derive("Level", CoreComponent.SceneComponent());

        Assert.Contains($"{RootType(derivedEntity)};", filter.Filters);
        Assert.Contains($"{RootType(derivedDrawable)};", filter.Filters);
        Assert.DoesNotContain($"{RootType(derivedScene)};", filter.Filters);
    }

    [Fact]
    public void EntityCode_WithoutBaseComponent_InheritsFromEngineType()
    {
        var component = CoreComponent.EntityComponent();
        var code = new EntityCode(component);

        Assert.Equal(EntityName, code.BaseClass);
        Assert.True(code.IsBaseComponentType);
    }

    [Fact]
    public void EntityCode_WithBaseComponent_InheritsFromBaseComponent()
    {
        var component = Derive("Player", CoreComponent.EntityComponent());
        var code = new EntityCode(component);

        Assert.Equal(EntityName, code.BaseClass);
        Assert.True(code.IsBaseComponentType);

        var nested = Derive("Boss", component);
        var nestedCode = new EntityCode(nested);

        Assert.Equal("Game.Player", nestedCode.BaseClass);
        Assert.False(nestedCode.IsBaseComponentType);
    }

    [Fact]
    public void DrawableCode_WithoutBaseComponent_InheritsFromEngineType()
    {
        var code = new DrawableCode(CoreComponent.DrawableComponent());

        Assert.Equal(DrawableName, code.BaseClass);
        Assert.True(code.IsBaseComponentType);
    }

    [Fact]
    public void PublicChildren_AreGeneratedOnlyForEntitiesAndDrawables()
    {
        var parent = Derive("Player", CoreComponent.EntityComponent());
        var publicEntity = Child("Weapon", CoreComponent.EntityComponent(), true);
        var publicDrawable = Child("Sprite", CoreComponent.DrawableComponent(), true);
        var privateDrawable = Child("Shadow", CoreComponent.DrawableComponent(), false);
        var publicScene = Child("Level", CoreComponent.SceneComponent(), true);
        parent.Children.AddRange(new[] { publicEntity, publicDrawable, privateDrawable, publicScene });

        var names = GetPublicChildNames(new EntityCode(parent));

        Assert.Equal(new[] { "Sprite", "Weapon" }, names.Values.OrderBy(n => n).ToArray());
    }

    [Fact]
    public void PublicChildren_OfDerivedEntityChildren_AreCollected()
    {
        var parent = Derive("Level", CoreComponent.EntityComponent());
        var enemy = Child("Enemy", CoreComponent.EntityComponent(), true);
        var enemySprite = Child("EnemySprite", CoreComponent.DrawableComponent(), true);
        enemy.Children.Add(enemySprite);
        parent.Children.Add(enemy);

        var names = GetPublicChildNames(new EntityCode(parent));

        Assert.Equal(new[] { "Enemy", "EnemySprite" }, names.Values.OrderBy(n => n).ToArray());
    }

    private static Component Derive(string name, Component baseComponent)
    {
        return new Component
        {
            ClassName = name,
            Namespace = "Game",
            BaseInheritedComponent = baseComponent
        };
    }

    private static Component Child(string name, Component baseComponent, bool isPublic)
    {
        var child = Derive(name, baseComponent);
        child.Public = isPublic;
        return child;
    }

    private static Dictionary<Component, string> GetPublicChildNames(ComponentTypeCode code)
    {
        var method = typeof(ComponentTypeCode).GetMethod("GetPublicChildNames", BindingFlags.NonPublic | BindingFlags.Instance);
        return (Dictionary<Component, string>)method.Invoke(code, null);
    }
}
