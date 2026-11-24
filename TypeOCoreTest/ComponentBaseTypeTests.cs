#nullable enable annotations

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Avalonia;
using TypeD;
using TypeD.Code;
using TypeD.Helpers;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeDCore.Code.Drawable;
using TypeDCore.Code.Entity;
using TypeDCore.Code.Game;
using TypeDCore.Components;
using TypeDCore.Models.Data.Hooks;
using TypeDCore.Models.Data;
using TypeDCore.Models.Data.PropertyDefinitions;
using TypeOEngine.Typedeaf.Core.Common;
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
    private const string WindowName = "TypeOEngine.Typedeaf.Core.Engine.Graphics.Window";

    [Fact]
    public void EnginePropertyTemplates_LoadWithoutEditorAssembly()
    {
        AppBuilder.Configure<Application>().UsePlatformDetect().SetupWithoutStarting();
        Assert.NotNull(new Vec2PropertyDefinition().CreateTemplate());
        Assert.NotNull(new Vec2iPropertyDefinition().CreateTemplate());
        Assert.NotNull(new ColorPropertyDefinition().CreateTemplate());
        Assert.NotNull(new ContentPropertyDefinition(null).CreateTemplate());
    }

    [Fact]
    public void SharedMoveRules_PreserveCoreComponentChildFilters()
    {
        var parent = CoreComponent.SceneComponent();
        var node = CoreComponent.EntityComponent();
        var sibling = CoreComponent.EntityComponent();
        typeof(Component).GetProperty(nameof(Component.ParentComponent)).SetValue(node, parent);
        typeof(Component).GetProperty(nameof(Component.ParentComponent)).SetValue(sibling, parent);
        parent.Children.AddRange(new[] { node, sibling });

        Assert.True(TypeD.Models.ComponentMoveRules.TryResolve(node, sibling, MovePosition.After, out var destination, out var index));
        Assert.Same(parent, destination);
        Assert.Equal(1, index);
        Assert.True(TypeDCore.Commands.ComponentMoveCommand.TryResolve(node, sibling, MovePosition.After, out var commandDestination, out var commandIndex));
        Assert.Same(destination, commandDestination);
        Assert.Equal(index, commandIndex);
    }

    [Theory]
    [InlineData(true, 70, 160)]
    [InlineData(false, 88, 177)]
    public void ClipboardPosition_UsesViewerOriginOrAncestorPositions(bool targetFound, double expectedX, double expectedY)
    {
        var ancestor = new Component();
        ancestor.Properties.Add(new Property { Name = "Position", Type = typeof(Vec2), Value = new Vec2(2, 3) });
        var parent = new Component();
        parent.Properties.Add(new Property { Name = "Position", Type = typeof(Vec2), Value = new Vec2(10, 20) });
        typeof(Component).GetProperty(nameof(Component.ParentComponent)).SetValue(parent, ancestor);
        var node = new Component();
        node.Properties.Add(new Property { Name = "Position", Type = typeof(Vec2), Value = Vec2.Zero });
        var hooks = (IHookModel)Activator.CreateInstance(typeof(TypeDInit).Assembly.GetType("TypeD.Models.HookModel", throwOnError: true), nonPublic: true);
        hooks.AddHook<ViewerHitTestHook>(hook =>
        {
            Assert.Same(parent, hook.Target);
            hook.TargetFound = targetFound;
            hook.OriginX = 30;
            hook.OriginY = 40;
        });
        var initializerType = typeof(CoreComponent).Assembly.GetType("TypeDCore.TypeDCoreInitializer", throwOnError: true);
        var initializer = (TypeDModuleInitializer)Activator.CreateInstance(initializerType, nonPublic: true);
        typeof(TypeDModuleInitializer).GetProperty(nameof(TypeDModuleInitializer.Hooks)).SetValue(initializer, hooks);

        initializerType.GetMethod("PositionPastedNode", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(initializer,
            new object[] { new NodeClipboardPositionHook { Parent = parent, Node = node, X = 100, Y = 200 } });

        Assert.Equal(new Vec2(expectedX, expectedY), node.Properties[0].Value);
    }

    // The only place that reads how the root type is exposed, switch it when the API changes.
    private static string? RootType(Component component) => component.RootBaseComponent?.FullName;

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
    [InlineData("Scene", true, true)]
    [InlineData("Drawable", false, true)]
    public void ChildFilter_AcceptsExpectedRootTypes(string parentName, bool acceptsEntity, bool acceptsDrawable)
    {
        var parent = (Func<Component>)BaseDefinitions().First(d => (string)d[0] == parentName)[1];
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

    [Fact]
    public void GameTemplate_AllowsAtMostOneChildPerSceneType()
    {
        var game = CoreComponent.GameComponent();
        var level = Child("Level", CoreComponent.SceneComponent(), false);
        var menu = Child("Menu", CoreComponent.SceneComponent(), false);
        var template = game.Template;
        typeof(TypeD.Components.ComponentTemplate).GetProperty(nameof(TypeD.Components.ComponentTemplate.Component)).SetValue(template, game);
        Assert.True(template.CanAddChild(level));
        game.Children.Add(level);

        Assert.False(template.CanAddChild(level));
        Assert.True(template.CanAddChild(menu));

        level.Removed = true;
        Assert.True(template.CanAddChild(level));
    }

    [Fact]
    public void GameCode_CreatesSceneChildrenAndAppliesOnlyPropertyOverrides()
    {
        var game = Derive("MyGame", CoreComponent.GameComponent());
        var first = Child("Level", CoreComponent.SceneComponent(), false);
        first.Namespace = "Game.Levels";
        first.Properties.Add(new Property { Name = "Pause", Type = typeof(bool), Value = true });
        first.Properties.Add(new Property { Name = "Hide", Type = typeof(bool), Value = false });
        first.OveriddenProperties.Add(new Property { Name = "Hide", Value = false });
        first.Properties.Add(new Property { Name = "ReadOnly", Type = typeof(int), Value = 1, ReadOnly = true });
        var second = Child("Menu", CoreComponent.SceneComponent(), false);
        second.Properties.Add(new Property { Name = "Hide", Type = typeof(bool), Value = true });
        var removed = Child("Removed", CoreComponent.SceneComponent(), false);
        removed.Removed = true;
        game.Children.AddRange(new[] { first, Child("Other", CoreComponent.GameComponent(), false), removed, second });

        var output = GenerateGameCode(game, second);

        Assert.Contains("var scene = Scenes.CreateScene<Game.Levels.Level>();", output);
        Assert.Contains("scene.Pause = true;", output);
        Assert.Contains("var scene = Scenes.CreateScene<Game.Menu>();", output);
        Assert.Contains("scene.Hide = true;", output);
        Assert.DoesNotContain("scene.Hide = false;", output);
        Assert.DoesNotContain("scene.ReadOnly =", output);
        Assert.DoesNotContain("Game.Removed>", output);
        Assert.DoesNotContain("Game.Other>", output);
        Assert.True(output.IndexOf("InternalInitialize();", StringComparison.Ordinal) < output.IndexOf("CreateScene<Game.Levels.Level>", StringComparison.Ordinal));
    }

    [Fact]
    public void GameCode_SetsTheStartingSceneAfterAllScenesAreCreated()
    {
        var game = Derive("MyGame", CoreComponent.GameComponent());
        var first = Child("Level", CoreComponent.SceneComponent(), false);
        var second = Child("Menu", CoreComponent.SceneComponent(), false);
        game.Children.AddRange(new[] { first, second });

        var output = GenerateGameCode(game, first);

        Assert.Equal(1, Regex.Matches(output, "Scenes.SetScene<").Count);
        Assert.Contains("Scenes.SetScene<Game.Level>();", output);
        Assert.True(output.IndexOf("CreateScene<Game.Menu>", StringComparison.Ordinal) < output.IndexOf("Scenes.SetScene<Game.Level>", StringComparison.Ordinal));
        Assert.DoesNotContain("StartingScene =", output);
    }

    [Fact]
    public void GameCode_DoesNotSetASceneWhenTheStartingSceneIsMissing()
    {
        var game = Derive("MyGame", CoreComponent.GameComponent());
        var scene = Child("Level", CoreComponent.SceneComponent(), false);
        var other = Child("Other", CoreComponent.SceneComponent(), false);
        game.Children.Add(scene);

        Assert.DoesNotContain("Scenes.SetScene<", GenerateGameCode(game, other));
        scene.Removed = true;
        Assert.DoesNotContain("Scenes.SetScene<", GenerateGameCode(game, scene));
        Assert.Equal(new string[0], StartingScene.Options(game));
    }

    [Fact]
    public void StartingSceneProperty_OffersTheScenesOfTheGame()
    {
        var game = Derive("MyGame", CoreComponent.GameComponent());
        game.Children.AddRange(new[] { Child("Level", CoreComponent.SceneComponent(), false), Child("Player", CoreComponent.EntityComponent(), false) });
        var property = Property.CreateDynamicComboBox(StartingScene.PropertyName, StartingScene.Options);

        Assert.Equal(new[] { "Game.Level" }, property.DynamicOptions(game));
        Assert.False(property.GenerateCode);
        Assert.Same(property.DynamicOptions, new Property(property).DynamicOptions);
    }

    [Fact]
    public void GameCode_WithoutWindows_AddsNoWindowCode()
    {
        var game = Derive("MyGame", CoreComponent.GameComponent());
        var scene = Child("Level", CoreComponent.SceneComponent(), false);
        game.Children.Add(scene);

        var output = GenerateGameCode(game, scene);

        Assert.DoesNotContain("WindowService", output);
        Assert.DoesNotContain("MainWindow", output);
    }

    [Fact]
    public void GameCode_FirstWindowIsMainWindowAndOnlyAWindowService()
    {
        var game = Derive("MyGame", CoreComponent.GameComponent());
        var scene = Child("Level", CoreComponent.SceneComponent(), false);
        game.Children.AddRange(new[] { scene, WindowChild("Main \"Window\"", 10, 20, 800, 600) });

        var output = GenerateGameCode(game, scene);

        Assert.Contains("public TypeOEngine.Typedeaf.Desktop.Engine.Services.WindowService WindowService { get; set; }", output);
        Assert.Contains("MainWindow = WindowService.CreateWindow(\"Main \\\"Window\\\"\", new TypeOEngine.Typedeaf.Core.Common.Vec2i(10, 20), new TypeOEngine.Typedeaf.Core.Common.Vec2i(800, 600));", output);
        Assert.DoesNotContain("Windows", output);
        Assert.True(output.IndexOf("MainWindow =", StringComparison.Ordinal) < output.IndexOf("CreateScene<", StringComparison.Ordinal));
    }

    [Fact]
    public void GameCode_AdditionalWindowsAreAddedToAList()
    {
        var game = Derive("MyGame", CoreComponent.GameComponent());
        var scene = Child("Level", CoreComponent.SceneComponent(), false);
        var removed = WindowChild("Removed", 0, 0, 1, 1);
        removed.Removed = true;
        game.Children.AddRange(new[] { scene, WindowChild("A", 0, 0, 1, 1), removed, WindowChild("B", 1, 2, 3, 4), WindowChild("C", 5, 6, 7, 8) });

        var output = GenerateGameCode(game, scene);

        Assert.Contains("List<TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces.IWindow> Windows", output);
        Assert.Equal(1, Regex.Matches(output, "MainWindow =").Count);
        Assert.Contains("Windows.Add(WindowService.CreateWindow(\"B\"", output);
        Assert.Contains("Windows.Add(WindowService.CreateWindow(\"C\"", output);
        Assert.DoesNotContain("Removed", output);
    }

    [Fact]
    public void WindowComponent_CanOnlyBeAddedToAGame()
    {
        var window = CoreComponent.WindowComponent();
        Assert.Equal(WindowName, RootType(window));
        Assert.False(new FilterHelperProbe(CoreComponent.EntityComponent()).Accepts(window));
        Assert.False(new FilterHelperProbe(CoreComponent.SceneComponent()).Accepts(window));
        Assert.False(new FilterHelperProbe(CoreComponent.DrawableComponent()).Accepts(window));
        Assert.False(new FilterHelperProbe(window).Accepts(CoreComponent.EntityComponent()));
        Assert.True(new FilterHelperProbe(CoreComponent.GameComponent()).Accepts(window));
        Assert.False(window.Template.CanAddChild(CoreComponent.EntityComponent()));
    }

    private sealed class FilterHelperProbe(Component parent)
    {
        public bool Accepts(Component child)
        {
            var filter = new FilterHelper();
            parent.Template.ChildrenFilter(filter);
            return filter.Filters.Contains($"{child.RootBaseComponent.FullName};");
        }
    }

    private static Component WindowChild(string title, int x, int y, int width, int height)
    {
        var window = Child("Window", CoreComponent.WindowComponent(), false);
        window.Properties.Add(new Property { Name = "Title", Type = typeof(string), Value = title });
        window.Properties.Add(new Property { Name = "Position", Type = typeof(Vec2i), Value = new Vec2i(x, y) });
        window.Properties.Add(new Property { Name = "Size", Type = typeof(Vec2i), Value = new Vec2i(width, height) });
        return window;
    }

    private static string GenerateGameCode(Component game, Component startScene)
    {
        game.Properties.Add(Property.CreateDynamicComboBox(StartingScene.PropertyName, StartingScene.Options));
        game.Properties[^1].Value = startScene.FullName;
        var provider = DispatchProxy.Create<IComponentProvider, SceneProviderProxy>();
        var resources = DispatchProxy.Create<IResourceModel, ResourceProxy>();
        ((ResourceProxy)resources).Provider = provider;
        var project = (Project)RuntimeHelpers.GetUninitializedObject(typeof(Project));
        var code = new TestGameCode(game);
        typeof(Codalyzer).GetProperty(nameof(Codalyzer.Resources)).SetValue(code, resources);
        typeof(Codalyzer).GetProperty(nameof(Codalyzer.Project)).SetValue(code, project);
        code.GenerateGame();
        return code.Output;
    }

    public class SceneProviderProxy : DispatchProxy
    {
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            throw new NotSupportedException(targetMethod.Name);
        }
    }

    public class ResourceProxy : DispatchProxy
    {
        public IComponentProvider Provider { get; set; }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (targetMethod.Name == nameof(IResourceModel.Get))
                return targetMethod.GetGenericArguments()[0] == typeof(IComponentProvider) ? Provider : null;
            throw new NotSupportedException(targetMethod.Name);
        }
    }

    private sealed class TestGameCode(Component component) : GameCode(component)
    {
        public string Output => TypeDFile.Output.ToString();

        public void GenerateGame()
        {
            TypeDFile.AutoGeneratedFile = false;
            Writer.TargetFile = TypeDFile;
            InitTypeDClass();
            Generate();
        }
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
        var method = typeof(ComponentTypeCode).GetMethod("GetPublicChildNames", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Could not find the public-child-name helper.");
        return method.Invoke(code, null) as Dictionary<Component, string>
            ?? throw new InvalidOperationException("The public-child-name helper returned an unexpected result.");
    }
}
