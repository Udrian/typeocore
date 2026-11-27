#nullable enable annotations

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Themes.Fluent;
using Avalonia.VisualTree;
using TypeD;
using TypeD.Code;
using TypeD.Helpers;
using TypeD.Models.Data;
using TypeD.Models.Data.PropertyDefinitions;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeD.ViewModel;
using TypeDCore.Code.Drawable;
using TypeDCore.Code.Entity;
using TypeDCore.Code.Game;
using TypeDCore.Components;
using TypeDCore.Models.Data.Hooks;
using TypeDCore.Models.Data;
using TypeDCore.Models.Data.PropertyDefinitions;
using TypeDCore.View.Dialogs.Project;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Attributes;
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
    private const string DrawStackName = "TypeOEngine.Typedeaf.Core.Engine.DrawStack";

    [Fact]
    public void AddPropertyGroups_IncludeComponentPropertiesAndCustomPropertiesAcrossTheTree()
    {
        var baseComponent = new Component();
        baseComponent.Properties.Add(new Property { Group = "Transform" });
        var first = new Component { BaseInheritedComponent = baseComponent };
        first.Properties.Add(new Property { Group = "Stats" });
        first.CustomProperties.Add(new CustomProperty { Group = "Private", Access = CustomPropertyAccess.Private });
        var second = new Component();
        second.CustomProperties.Add(new CustomProperty { Group = "Stats", Access = CustomPropertyAccess.Protected });
        var child = new Component();
        child.Properties.Add(new Property { Group = "Rendering" });
        second.Children.Add(child);

        var viewModel = typeof(CoreComponent).Assembly.GetType("TypeDCore.ViewModel.Dialogs.Project.AddCustomPropertyViewModel", throwOnError: true);
        var getGroups = viewModel.GetMethod("GetGroups", BindingFlags.Static | BindingFlags.NonPublic);
        var groups = (List<string>)getGroups.Invoke(null, new object[] { new[] { first, second, baseComponent } });

        Assert.Equal(new[] { "Private", "Rendering", "Stats", "Transform" }, groups);
    }

    [Fact]
    public void EditPropertyDialog_LoadsExistingValuesWithoutChangingOriginal()
    {
        var existing = new CustomProperty
        {
            Name = "Health",
            TypeName = typeof(int).FullName,
            DefaultValue = 42,
            Description = "Hit points",
            Group = "Stats",
            Access = CustomPropertyAccess.Public
        };
        var viewModelType = typeof(CoreComponent).Assembly.GetType("TypeDCore.ViewModel.Dialogs.Project.AddCustomPropertyViewModel", throwOnError: true);
        var viewModel = Activator.CreateInstance(viewModelType, new object[]
        {
            null, new IPropertyDefinition[] { new TestIntDefinition() }, Array.Empty<string>(), Array.Empty<Component>(), existing
        });

        Assert.True((bool)viewModelType.GetMethod("Validate").Invoke(viewModel, null));
        var copy = (CustomProperty)viewModelType.GetMethod("Build").Invoke(viewModel, null);
        Assert.Equal(existing.Name, copy.Name);
        Assert.Equal(existing.TypeName, copy.TypeName);
        Assert.Equal(existing.DefaultValue, copy.DefaultValue);
        Assert.Equal(existing.Description, copy.Description);
        Assert.Equal(existing.Group, copy.Group);
        Assert.Equal(existing.Access, copy.Access);
        Assert.NotSame(existing, copy);
    }

    [Fact]
    public void EditPropertyDialog_DoesNotReplaceAnUnavailableType()
    {
        var existing = new CustomProperty { Name = "Health", TypeName = "Unavailable.Type", DefaultValue = 42 };
        var viewModelType = typeof(CoreComponent).Assembly.GetType("TypeDCore.ViewModel.Dialogs.Project.AddCustomPropertyViewModel", throwOnError: true);
        var viewModel = Activator.CreateInstance(viewModelType, new object[]
        {
            null, new IPropertyDefinition[] { new TestIntDefinition() }, Array.Empty<string>(), Array.Empty<Component>(), existing
        });

        Assert.False((bool)viewModelType.GetMethod("Validate").Invoke(viewModel, null));
        Assert.Null(viewModelType.GetProperty("SelectedType").GetValue(viewModel));
    }

    [Fact]
    public void RemoveCustomProperty_RemovesOverridesAndUndoRestoresThem()
    {
        var owner = new Component { Namespace = "Game", ClassName = "Actor" };
        var custom = new CustomProperty { Name = "Health", TypeName = typeof(int).FullName, DefaultValue = 10 };
        owner.CustomProperties.Add(custom);
        var ownerProperty = new Property { Name = "Health", DeclaringTypeName = owner.FullName, Value = 10 };
        owner.Properties.Add(ownerProperty);
        owner.OveriddenProperties.Add(new Property(ownerProperty));

        var derived = new Component { Namespace = "Game", ClassName = "Enemy", BaseInheritedComponent = owner };
        var derivedProperty = new Property { Name = "Health", DeclaringTypeName = owner.FullName, Value = 25 };
        derived.Properties.Add(derivedProperty);
        derived.OveriddenProperties.Add(new Property { Name = "Health", DeclaringTypeName = owner.FullName, Value = 10 });
        var unrelated = new Property { Name = "Health", DeclaringTypeName = "Game.Other", Value = 99 };
        var other = new Component { Namespace = "Game", ClassName = "Other" };
        other.Properties.Add(unrelated);
        var parent = new Component { Namespace = "Game", ClassName = "Scene" };
        var instance = new Component { Namespace = "Game", ClassName = "Actor" };
        instance.CustomProperties.Add(new CustomProperty(custom));
        var instanceProperty = new Property { Name = "Health", DeclaringTypeName = owner.FullName, Value = 30 };
        instance.Properties.Add(instanceProperty);
        instance.OveriddenProperties.Add(new Property { Name = "Health", DeclaringTypeName = owner.FullName, Value = 10 });
        parent.Children.Add(instance);

        var planType = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.RemoveCustomPropertyPlan", throwOnError: true);
        var plan = Activator.CreateInstance(planType, new object[] { new[] { owner, derived, other, parent }, owner.FullName, "Health" });
        planType.GetMethod("Apply").Invoke(plan, null);

        Assert.Empty(owner.CustomProperties);
        Assert.Empty(owner.Properties);
        Assert.Empty(derived.Properties);
        Assert.Empty(derived.OveriddenProperties);
        Assert.Empty(instance.Properties);
        Assert.Empty(instance.CustomProperties);
        Assert.Same(unrelated, Assert.Single(other.Properties));
        Assert.True(owner.IsRemovedCustomProperty(owner.FullName, "Health"));
        Assert.True(derived.IsRemovedCustomProperty(owner.FullName, "Health"));

        planType.GetMethod("Undo").Invoke(plan, null);
        Assert.Same(custom, Assert.Single(owner.CustomProperties));
        Assert.Same(ownerProperty, Assert.Single(owner.Properties));
        Assert.Same(derivedProperty, Assert.Single(derived.Properties));
        Assert.Same(instanceProperty, Assert.Single(instance.Properties));
        Assert.Equal(30, instance.Properties[0].Value);
        Assert.Equal(10, Assert.Single(derived.OveriddenProperties).Value);
        Assert.False(owner.IsRemovedCustomProperty(owner.FullName, "Health"));

        planType.GetMethod("Apply").Invoke(plan, null);
        Assert.Empty(owner.CustomProperties);
        Assert.True(owner.IsRemovedCustomProperty(owner.FullName, "Health"));
    }

    [Fact]
    public void EditCustomProperty_UpdatesOverridesAndRestoresThemOnUndo()
    {
        var owner = new Component { Namespace = "Game", ClassName = "Actor" };
        var old = new CustomProperty { Name = "Health", TypeName = typeof(int).FullName, DefaultValue = 10, Group = "Stats" };
        owner.CustomProperties.Add(old);
        owner.Properties.Add(new Property { Name = old.Name, Type = typeof(int), DeclaringTypeName = owner.FullName, Value = 10 });
        owner.OveriddenProperties.Add(new Property(owner.Properties[0]));
        var derived = new Component { Namespace = "Game", ClassName = "Enemy", BaseInheritedComponent = owner };
        derived.Properties.Add(new Property { Name = old.Name, Type = typeof(int), DeclaringTypeName = owner.FullName, Value = 25 });
        derived.OveriddenProperties.Add(new Property { Name = old.Name, Type = typeof(int), DeclaringTypeName = owner.FullName, Value = 10 });
        var replacement = new CustomProperty { Name = "HitPoints", TypeName = typeof(double).FullName, DefaultValue = 12.0, Group = "Combat" };
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.EditCustomPropertyPlan", throwOnError: true);
        var plan = Activator.CreateInstance(type, new object[] { new[] { owner, derived }, owner.FullName, old, replacement, typeof(double) });

        type.GetMethod("Apply").Invoke(plan, null);
        Assert.Equal("HitPoints", old.Name);
        Assert.Equal("Combat", owner.Properties[0].Group);
        Assert.Equal(12.0, owner.Properties[0].Value);
        Assert.Equal(25.0, derived.Properties[0].Value);
        Assert.Equal(12.0, derived.OveriddenProperties[0].Value);
        Assert.True(owner.IsRemovedCustomProperty(owner.FullName, "Health"));

        type.GetMethod("Undo").Invoke(plan, null);
        Assert.Equal("Health", old.Name);
        Assert.Equal(10, owner.Properties[0].Value);
        Assert.Equal(25, derived.Properties[0].Value);
        Assert.False(owner.IsRemovedCustomProperty(owner.FullName, "Health"));
    }

    [Fact]
    public void EditCustomProperty_UsesDefaultForUnconvertibleValues()
    {
        var owner = new Component { Namespace = "Game", ClassName = "Actor" };
        var old = new CustomProperty { Name = "Label", TypeName = typeof(string).FullName, DefaultValue = "10" };
        owner.CustomProperties.Add(old);
        owner.Properties.Add(new Property { Name = old.Name, Type = typeof(string), DeclaringTypeName = owner.FullName, Value = "invalid" });
        owner.OveriddenProperties.Add(new Property { Name = old.Name, Type = typeof(string), DeclaringTypeName = owner.FullName, Value = "10" });
        var replacement = new CustomProperty { Name = "Label", TypeName = typeof(int).FullName, DefaultValue = 1 };
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.EditCustomPropertyPlan", throwOnError: true);
        var plan = Activator.CreateInstance(type, new object[] { new[] { owner }, owner.FullName, old, replacement, typeof(int) });
        type.GetMethod("Apply").Invoke(plan, null);
        Assert.Equal(1, owner.Properties[0].Value);
        type.GetMethod("Undo").Invoke(plan, null);
        Assert.Equal("invalid", owner.Properties[0].Value);
        type.GetMethod("Apply").Invoke(plan, null);
        Assert.Equal(1, owner.Properties[0].Value);
    }

    [Fact]
    public void EditCustomProperty_DefaultChangePreservesDifferentInheritedBaseline()
    {
        var owner = new Component { Namespace = "Game", ClassName = "Actor" };
        var old = new CustomProperty { Name = "Health", TypeName = typeof(int).FullName, DefaultValue = 10 };
        owner.CustomProperties.Add(old);
        owner.Properties.Add(new Property { Name = old.Name, Type = typeof(int), DeclaringTypeName = owner.FullName, Value = 10 });
        owner.OveriddenProperties.Add(new Property(owner.Properties[0]));
        var derived = new Component { Namespace = "Game", ClassName = "Enemy", BaseInheritedComponent = owner };
        derived.Properties.Add(new Property { Name = old.Name, Type = typeof(int), DeclaringTypeName = owner.FullName, Value = 8 });
        derived.OveriddenProperties.Add(new Property(derived.Properties[0]));
        var replacement = new CustomProperty { Name = old.Name, TypeName = old.TypeName, DefaultValue = 12 };
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.EditCustomPropertyPlan", throwOnError: true);
        var plan = Activator.CreateInstance(type, new object[] { new[] { owner, derived }, owner.FullName, old, replacement, typeof(int) });

        type.GetMethod("Apply").Invoke(plan, null);
        Assert.Equal(12, owner.Properties[0].Value);
        Assert.Equal(12, owner.OveriddenProperties[0].Value);
        Assert.Equal(8, derived.Properties[0].Value);
        Assert.Equal(8, derived.OveriddenProperties[0].Value);
        Assert.False(derived.HasPropertyOverridesFor("Health"));

        type.GetMethod("Undo").Invoke(plan, null);
        Assert.Equal(10, owner.OveriddenProperties[0].Value);
        Assert.Equal(8, derived.OveriddenProperties[0].Value);
    }

    [Fact]
    public void EditCustomProperty_UpdatesUnconvertibleBaselinesWithoutExtraConfirmation()
    {
        var owner = new Component { Namespace = "Game", ClassName = "Actor" };
        var old = new CustomProperty { Name = "Label", TypeName = typeof(string).FullName, DefaultValue = "0" };
        owner.CustomProperties.Add(old);
        owner.Properties.Add(new Property { Name = old.Name, Type = typeof(string), DeclaringTypeName = owner.FullName, Value = "invalid" });
        owner.OveriddenProperties.Add(new Property { Name = old.Name, Type = typeof(string), DeclaringTypeName = owner.FullName, Value = "invalid" });
        var derived = new Component { Namespace = "Game", ClassName = "Enemy", BaseInheritedComponent = owner };
        derived.Properties.Add(new Property { Name = old.Name, Type = typeof(string), DeclaringTypeName = owner.FullName, Value = "25" });
        derived.OveriddenProperties.Add(new Property { Name = old.Name, Type = typeof(string), DeclaringTypeName = owner.FullName, Value = "other" });
        var replacement = new CustomProperty { Name = "Label", TypeName = typeof(int).FullName, DefaultValue = 3 };
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.EditCustomPropertyPlan", throwOnError: true);
        var plan = Activator.CreateInstance(type, new object[] { new[] { owner, derived }, owner.FullName, old, replacement, typeof(int) });
        var values = ((System.Collections.IEnumerable)type.GetProperty("Values").GetValue(plan)).Cast<object>();
        var value = values.Single(item => ReferenceEquals(item.GetType().GetProperty("Property").GetValue(item), owner.Properties[0]));
        value.GetType().GetProperty("Replacement").SetValue(value, 42);

        type.GetMethod("Apply").Invoke(plan, null);
        Assert.Equal(42, owner.Properties[0].Value);
        Assert.Equal(42, owner.OveriddenProperties[0].Value);
        Assert.False(owner.HasPropertyOverridesFor("Label"));
        Assert.Equal(25, derived.Properties[0].Value);
        Assert.Equal(3, derived.OveriddenProperties[0].Value);
        Assert.True(derived.HasPropertyOverridesFor("Label"));

        type.GetMethod("Undo").Invoke(plan, null);
        Assert.Equal("invalid", owner.OveriddenProperties[0].Value);
        Assert.Equal("other", derived.OveriddenProperties[0].Value);
        type.GetMethod("Apply").Invoke(plan, null);
        Assert.Equal(42, owner.OveriddenProperties[0].Value);
        Assert.Equal(3, derived.OveriddenProperties[0].Value);
    }

    [Fact]
    public void EditCustomProperty_RemovingPublicAccessRemovesValuesAndUndoRestoresThem()
    {
        var owner = new Component { Namespace = "Game", ClassName = "Actor" };
        var old = new CustomProperty { Name = "Health", TypeName = typeof(int).FullName, DefaultValue = 10 };
        owner.CustomProperties.Add(old);
        owner.Properties.Add(new Property { Name = old.Name, Type = typeof(int), DeclaringTypeName = owner.FullName, Value = 20 });
        owner.OveriddenProperties.Add(new Property { Name = old.Name, Type = typeof(int), DeclaringTypeName = owner.FullName, Value = 10 });
        var replacement = new CustomProperty(old) { Access = CustomPropertyAccess.Private };
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.EditCustomPropertyPlan", throwOnError: true);
        var plan = Activator.CreateInstance(type, new object[] { new[] { owner }, owner.FullName, old, replacement, typeof(int) });
        type.GetMethod("Apply").Invoke(plan, null);
        Assert.Empty(owner.Properties);
        Assert.Empty(owner.OveriddenProperties);
        Assert.Equal(CustomPropertyAccess.Private, old.Access);
        type.GetMethod("Undo").Invoke(plan, null);
        Assert.Equal(20, Assert.Single(owner.Properties).Value);
        Assert.Equal(CustomPropertyAccess.Public, old.Access);
    }

    [Fact]
    public void EditCustomProperty_RejectsConflictingDerivedProperty()
    {
        var owner = new Component { Namespace = "Game", ClassName = "Actor" };
        var old = new CustomProperty { Name = "Health", TypeName = typeof(int).FullName, DefaultValue = 10 };
        owner.CustomProperties.Add(old);
        owner.Properties.Add(new Property { Name = old.Name, DeclaringTypeName = owner.FullName });
        var derived = new Component { Namespace = "Game", ClassName = "Enemy", BaseInheritedComponent = owner };
        derived.Properties.Add(new Property { Name = old.Name, DeclaringTypeName = owner.FullName });
        derived.CustomProperties.Add(new CustomProperty { Name = "HitPoints", Access = CustomPropertyAccess.Private });
        var replacement = new CustomProperty(old) { Name = "HitPoints" };
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.EditCustomPropertyPlan", throwOnError: true);
        Assert.IsType<InvalidOperationException>(Assert.Throws<TargetInvocationException>(() =>
            Activator.CreateInstance(type, new object[] { new[] { owner, derived }, owner.FullName, old, replacement, typeof(int) })).InnerException);
    }

    [Fact]
    public async Task EditCustomProperty_RenamesOnlyBoundReferencesAndSupportsUndo()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"TypeOEditProperty-{Guid.NewGuid():N}");
        var sourceDirectory = Path.Combine(directory, "Game");
        var generatedDirectory = Path.Combine(directory, "TypeO", "code");
        Directory.CreateDirectory(sourceDirectory);
        Directory.CreateDirectory(generatedDirectory);
        try
        {
            var projectFile = Path.Combine(sourceDirectory, "Game.csproj");
            File.WriteAllText(projectFile, "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup><ItemGroup><Compile Include=\"..\\TypeO\\code\\*.cs\" /></ItemGroup></Project>");
            var generated = Path.Combine(generatedDirectory, "Actor.typed.cs");
            File.WriteAllText(generated, "namespace Game; public partial class Actor { public int Health { get; set; } }");
            var source = Path.Combine(sourceDirectory, "Actor.cs");
            const string original = "namespace Game; public partial class Actor { public int Get(Other other) => Health + other.Health; public string Text = \"Health\"; } public class Other { public int Health { get; set; } }";
            File.WriteAllText(source, original);
            var start = new System.Diagnostics.ProcessStartInfo("dotnet")
            {
                WorkingDirectory = sourceDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            start.ArgumentList.Add("restore");
            start.ArgumentList.Add(projectFile);
            using var process = System.Diagnostics.Process.Start(start);
            var output = await process.StandardOutput.ReadToEndAsync();
            var errors = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            Assert.True(process.ExitCode == 0, output + errors);

            var project = (Project)RuntimeHelpers.GetUninitializedObject(typeof(Project));
            typeof(Project).GetProperty("Location").SetValue(project, directory);
            typeof(Project).GetProperty("ProjectName").SetValue(project, "Game");
            var modulePath = typeof(CoreComponent).Assembly.Location;
            var loader = new MemoryCompilerLoadContext(Path.GetDirectoryName(modulePath));
            using var moduleStream = File.OpenRead(modulePath);
            var module = loader.LoadFromStream(moduleStream);
            var type = module.GetType("TypeDCore.Commands.PropertyCodeRename", throwOnError: true);
            var task = (Task)type.GetMethod("CreateAsync").Invoke(null, new object[] { project, "Game.Actor", "Health", "HitPoints" });
            await task;
            var rename = task.GetType().GetProperty("Result").GetValue(task);
            type.GetMethod("Apply").Invoke(rename, null);
            Assert.Contains("=> HitPoints + other.Health", File.ReadAllText(source));
            Assert.Contains("\"Health\"", File.ReadAllText(source));
            Assert.Contains("int Health", File.ReadAllText(generated));
            var renamedSource = File.ReadAllText(source);
            File.AppendAllText(source, " // edited externally");
            Assert.IsType<IOException>(Assert.Throws<TargetInvocationException>(() => type.GetMethod("Undo").Invoke(rename, null)).InnerException);
            Assert.EndsWith("// edited externally", File.ReadAllText(source));
            File.WriteAllText(source, renamedSource);
            type.GetMethod("Undo").Invoke(rename, null);
            Assert.Equal(original, File.ReadAllText(source));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private sealed class MemoryCompilerLoadContext : System.Runtime.Loader.AssemblyLoadContext
    {
        private readonly string Folder;
        public MemoryCompilerLoadContext(string folder) => Folder = folder;

        protected override Assembly Load(AssemblyName name)
        {
            if (name.Name.StartsWith("Microsoft.CodeAnalysis", StringComparison.Ordinal)
                || name.Name.StartsWith("System.Composition", StringComparison.Ordinal))
            {
                var path = Path.Combine(Folder, name.Name + ".dll");
                if (File.Exists(path))
                {
                    using var stream = File.OpenRead(path);
                    return LoadFromStream(stream);
                }
            }
            return null;
        }
    }

    [Fact]
    public void RemoveCustomProperty_OnlyOffersActionForOwnEditableCustomProperties()
    {
        var component = new Component { Namespace = "Game", ClassName = "Actor" };
        component.CustomProperties.Add(new CustomProperty { Name = "Health" });
        var property = new Property { Name = "Health", DeclaringTypeName = component.FullName };
        component.Properties.Add(property);
        var command = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.RemoveCustomPropertyCommand", throwOnError: true);
        var canRemove = command.GetMethod("CanRemove", BindingFlags.Static | BindingFlags.NonPublic);
        bool CanRemove(Component owner, Property candidate) => (bool)canRemove.Invoke(null, new object[] { owner, candidate });

        Assert.True(CanRemove(component, property));
        property.DeclaringTypeName = "Game.Base";
        Assert.False(CanRemove(component, property));
        property.DeclaringTypeName = component.FullName;
        component.ReadOnly = true;
        Assert.False(CanRemove(component, property));
        component.ReadOnly = false;
        component.CustomProperties.Clear();
        Assert.False(CanRemove(component, property));
    }

    [Fact]
    public void PropertyContextMenu_OffersRemoveOnlyForOwnCustomProperty()
    {
        var initializerType = typeof(CoreComponent).Assembly.GetType("TypeDCore.TypeDCoreInitializer", throwOnError: true);
        var initializer = Activator.CreateInstance(initializerType, nonPublic: true);
        var commandType = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.RemoveCustomPropertyCommand", throwOnError: true);
        initializerType.GetProperty("RemoveCustomPropertyCommand", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(initializer, RuntimeHelpers.GetUninitializedObject(commandType));
        var editType = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.EditCustomPropertyCommand", throwOnError: true);
        initializerType.GetProperty("EditCustomPropertyCommand", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(initializer, RuntimeHelpers.GetUninitializedObject(editType));
        var component = new Component { Namespace = "Game", ClassName = "Actor" };
        component.CustomProperties.Add(new CustomProperty { Name = "Health" });
        var property = new Property { Name = "Health", DeclaringTypeName = component.FullName };
        component.Properties.Add(property);
        var hook = new PropertyContextMenuHook
        {
            Project = (Project)RuntimeHelpers.GetUninitializedObject(typeof(Project)),
            Component = component,
            Property = property
        };
        var handler = initializerType.GetMethod("PropertyContextMenuRequested", BindingFlags.Instance | BindingFlags.NonPublic);
        handler.Invoke(initializer, new object[] { hook });
        Assert.Equal(new[] { "Edit _Property", "Remove _Property" }, hook.Menu.Items.Select(item => item.Name));

        property.DeclaringTypeName = "Game.Base";
        var inherited = new PropertyContextMenuHook { Project = hook.Project, Component = component, Property = property };
        handler.Invoke(initializer, new object[] { inherited });
        Assert.Empty(inherited.Menu.Items);
    }

    [Fact]
    public void RemovePropertyPreview_FindsPossibleSourceReferencesWithoutBuildOutput()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"TypeORemoveProperty-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(directory, "bin"));
        try
        {
            File.WriteAllText(Path.Combine(directory, "Actor.cs"), "actor.Health = 10;");
            File.WriteAllText(Path.Combine(directory, "Other.cs"), "actor.HealthPoints = 10;");
            File.WriteAllText(Path.Combine(directory, "bin", "Generated.cs"), "actor.Health = 10;");
            var commandType = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.RemoveCustomPropertyCommand", throwOnError: true);
            var find = commandType.GetMethod("FindPossibleCodeReferences", BindingFlags.Static | BindingFlags.NonPublic);
            var paths = (IEnumerable<string>)find.Invoke(null, new object[] { directory, "Health" });

            Assert.Equal(new[] { "Actor.cs" }, paths.ToArray());
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void RemovedProperty_IsNotExtractedFromStaleCompiledType()
    {
        var type = typeof(CompiledPropertyOwner);
        var component = new Component
        {
            Namespace = type.Namespace,
            ClassName = type.FullName.Substring(type.Namespace.Length + 1)
        };
        component.RemovedCustomProperties.Add("Health");
        var initializerType = typeof(CoreComponent).Assembly.GetType("TypeDCore.TypeDCoreInitializer", throwOnError: true);
        var initializer = Activator.CreateInstance(initializerType, nonPublic: true);
        var modelType = typeof(TypeDInit).Assembly.GetType("TypeD.Models.ComponentModel", throwOnError: true);
        var propertyModelType = typeof(TypeDInit).Assembly.GetType("TypeD.Models.PropertyModel", throwOnError: true);
        initializerType.GetProperty("ComponentModel", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(initializer, Activator.CreateInstance(modelType, nonPublic: true));
        initializerType.GetProperty("PropertyModel", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(initializer, Activator.CreateInstance(propertyModelType, nonPublic: true));
        var extract = initializerType.GetMethod("ExtractProperties");

        var removed = new ExtractPropertiesHook(component);
        extract.Invoke(initializer, new object[] { removed });
        Assert.DoesNotContain(removed.Properties, property => property.Name == "Health");

        component.CustomProperties.Add(new CustomProperty { Name = "Health" });
        var readded = new ExtractPropertiesHook(component);
        extract.Invoke(initializer, new object[] { readded });
        Assert.Contains(readded.Properties, property => property.Name == "Health");

        component.CustomProperties[0].Access = CustomPropertyAccess.Private;
        var privateProperty = new ExtractPropertiesHook(component);
        extract.Invoke(initializer, new object[] { privateProperty });
        Assert.DoesNotContain(privateProperty.Properties, property => property.Name == "Health");
    }

    public sealed class CompiledPropertyOwner
    {
        [TypeOProperty]
        public int Health { get; set; }
    }

    private sealed class TestIntDefinition : IPropertyDefinition
    {
        public Type Type => typeof(int);
        public Type ValueType => typeof(int);
        public Type PropertyTemplateType => typeof(int);
        public JsonConverter JsonConverter => null;
        public IDataTemplate CreateTemplate() => null;
        public string ToCode(object value) => value.ToString();
        public PropertyNode CreateNode(Component component, Property property, Action<PropertyNode, object> valueChanged)
            => new Int32PropertyNode(component, property, valueChanged);
    }

    [Fact]
    public void EnginePropertyTemplates_LoadWithoutEditorAssembly()
    {
        AppBuilder.Configure<Application>().UsePlatformDetect().SetupWithoutStarting();
        VerifyCustomPropertyDialogLayout();
        var progressType = typeof(CoreComponent).Assembly.GetType("TypeDCore.View.Dialogs.Project.PropertyReferenceScanDialog", throwOnError: true);
        var progress = (Window)Activator.CreateInstance(progressType);
        Assert.True(progress.FindControl<ProgressBar>("scanProgress").IsIndeterminate);
        Assert.Contains("Scanning the project for property references", progress.FindControl<TextBlock>("scanMessage").Text);
        Assert.NotNull(new Vec2PropertyDefinition().CreateTemplate());
        Assert.NotNull(new Vec2iPropertyDefinition().CreateTemplate());
        Assert.NotNull(new ColorPropertyDefinition().CreateTemplate());
        Assert.NotNull(new ContentPropertyDefinition(null).CreateTemplate());

        var owner = new Component { Namespace = "Game", ClassName = "Actor" };
        var original = new CustomProperty { Name = "Health", TypeName = typeof(string).FullName, DefaultValue = "10" };
        owner.CustomProperties.Add(original);
        owner.Properties.Add(new Property { Name = "Health", Type = typeof(string), DeclaringTypeName = owner.FullName, Value = "invalid" });
        owner.OveriddenProperties.Add(new Property { Name = "Health", Type = typeof(string), DeclaringTypeName = owner.FullName, Value = "baseline-only" });
        var scene = new Component { Namespace = "Game", ClassName = "Scene" };
        var parent = new Component { Namespace = "Game", ClassName = "Container" };
        parent.Properties.Add(new Property { Name = "Name", Value = "Enemies" });
        parent.Properties.Add(new Property { Name = "ID", Value = "parent-id" });
        var child = new Component { Namespace = "Game", ClassName = "Actor" };
        child.Properties.Add(new Property { Name = "ID", Value = "child-id" });
        child.Properties.Add(new Property { Name = "Health", Type = typeof(string), DeclaringTypeName = owner.FullName, Value = "20" });
        child.OveriddenProperties.Add(new Property { Name = "Health", Type = typeof(string), DeclaringTypeName = owner.FullName, Value = "10" });
        parent.Children.Add(child);
        scene.Children.Add(parent);
        var derived = new Component { Namespace = "Game", ClassName = "Enemy", BaseInheritedComponent = owner };
        derived.Properties.Add(new Property { Name = "Health", Type = typeof(string), DeclaringTypeName = owner.FullName, Value = "invalid" });
        var game = new Component { Namespace = "Game", ClassName = "Game" };
        var sceneInstance = new Component { Namespace = "Game", ClassName = "Scene" };
        var parentInstance = new Component { Namespace = "Game", ClassName = "Container" };
        var inheritedChild = new Component { Namespace = "Game", ClassName = "Actor" };
        inheritedChild.Properties.Add(new Property { Name = "Health", Type = typeof(string), DeclaringTypeName = owner.FullName, Value = "20" });
        inheritedChild.OveriddenProperties.Add(new Property(inheritedChild.Properties[0]));
        parentInstance.Children.Add(inheritedChild);
        parentInstance.DefinitionChildren.Add(inheritedChild);
        sceneInstance.Children.Add(parentInstance);
        sceneInstance.DefinitionChildren.Add(parentInstance);
        game.Children.Add(sceneInstance);
        var roots = new[] { owner, scene, derived, game };
        var dialogType = typeof(CoreComponent).Assembly.GetType("TypeDCore.View.Dialogs.Project.PropertyChangePreviewDialog", throwOnError: true);
        var removeType = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.RemoveCustomPropertyPlan", throwOnError: true);
        var removePlan = Activator.CreateInstance(removeType, new object[] { roots, owner.FullName, "Health" });
        var removal = (Window)Activator.CreateInstance(dialogType, BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object[] { "Game.Actor.Health", removePlan, new[] { "Actor.cs" } }, null);
        Assert.Contains("Game.Actor.Health", removal.FindControl<TextBlock>("message").Text);
        Assert.Contains("manual changes", removal.FindControl<TextBlock>("message").Text);
        Assert.Equal("_Remove", removal.FindControl<Button>("confirmButton").Content);
        var groups = removal.FindControl<ItemsControl>("componentGroups").ItemsSource.Cast<object>().ToList();
        Assert.Equal(new[] { "Game.Scene", "Game.Enemy" },
            groups.Select(group => (string)group.GetType().GetProperty("Name").GetValue(group)));
        Assert.False(removal.FindControl<TextBlock>("noComponentReferences").IsVisible);
        var sceneRows = ((System.Collections.IEnumerable)groups[0].GetType().GetProperty("Rows").GetValue(groups[0])).Cast<object>().ToList();
        var sceneRow = Assert.Single(sceneRows);
        var nodePath = (string)sceneRow.GetType().GetProperty("NodePath").GetValue(sceneRow);
        Assert.Contains("Enemies", nodePath);
        Assert.Contains("Actor", nodePath);
        Assert.Contains(Environment.NewLine, nodePath);
        Assert.DoesNotContain("-id", nodePath);
        Assert.Equal("(removed)", sceneRow.GetType().GetProperty("NewValue").GetValue(sceneRow));
        Assert.Single(removal.FindControl<ItemsControl>("sources").ItemsSource.Cast<object>());
        var groupView = removal.FindControl<ItemsControl>("componentGroups").ItemTemplate.Build(groups[0]);
        groupView.DataContext = groups[0];
        groupView.Measure(new Size(900, 600));
        groupView.Arrange(new Rect(0, 0, 900, 600));
        Assert.Contains(groupView.GetVisualDescendants().OfType<TextBlock>(), text => text.Text == "Game.Scene");
        var rowView = Assert.Single(groupView.GetVisualDescendants().OfType<ItemsControl>()).ItemTemplate.Build(sceneRow);
        rowView.DataContext = sceneRow;
        rowView.Measure(new Size(900, 600));
        rowView.Arrange(new Rect(0, 0, 900, 600));
        Assert.Contains(rowView.GetVisualDescendants().OfType<TextBlock>(), text => text.Text == nodePath);
        Assert.Contains(rowView.GetVisualDescendants().OfType<TextBlock>(), text => text.Text == "(removed)");
        var derivedRows = ((System.Collections.IEnumerable)groups[1].GetType().GetProperty("Rows").GetValue(groups[1])).Cast<object>().ToList();
        Assert.Single(derivedRows);

        inheritedChild.Properties[0].Value = "25";
        var removalWithOverride = (Window)Activator.CreateInstance(dialogType, BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object[] { "Game.Actor.Health", removePlan, Array.Empty<string>() }, null);
        var groupsWithOverride = removalWithOverride.FindControl<ItemsControl>("componentGroups").ItemsSource.Cast<object>().ToList();
        var gameGroup = Assert.Single(groupsWithOverride, group => (string)group.GetType().GetProperty("Name").GetValue(group) == "Game.Game");
        var gameRow = Assert.Single(((System.Collections.IEnumerable)gameGroup.GetType().GetProperty("Rows").GetValue(gameGroup)).Cast<object>());
        Assert.Equal("25", gameRow.GetType().GetProperty("OldValue").GetValue(gameRow));
        inheritedChild.Properties[0].Value = "20";

        var replacement = new CustomProperty(original) { TypeName = typeof(int).FullName, DefaultValue = 1 };
        var planType = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.EditCustomPropertyPlan", throwOnError: true);
        var plan = Activator.CreateInstance(planType, new object[] { roots, owner.FullName, original, replacement, typeof(int) });
        var edit = (Window)Activator.CreateInstance(dialogType, BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object[] { plan, new TestIntDefinition(), Array.Empty<string>(), true, false, false }, null);
        var editGroups = edit.FindControl<ItemsControl>("componentGroups").ItemsSource.Cast<object>().ToList();
        Assert.DoesNotContain(editGroups, group => (string)group.GetType().GetProperty("Name").GetValue(group) == "Game.Game");
        Assert.Equal(new[] { "Game.Scene", "Game.Enemy" },
            editGroups.Select(group => (string)group.GetType().GetProperty("Name").GetValue(group)));
        var editDerivedRows = ((System.Collections.IEnumerable)editGroups[1].GetType().GetProperty("Rows").GetValue(editGroups[1])).Cast<object>().ToList();
        var valueRow = Assert.Single(editDerivedRows);
        Assert.DoesNotContain("could not be converted", edit.FindControl<TextBlock>("message").Text);
        Assert.Equal("invalid", valueRow.GetType().GetProperty("OldValue").GetValue(valueRow));
        var valueNode = (PropertyNode)valueRow.GetType().GetProperty("Node").GetValue(valueRow);
        Assert.Equal(1, valueNode.Value);
        dialogType.GetMethod("Apply_Click", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(edit, new object[] { edit, new RoutedEventArgs() });
        Assert.True(string.IsNullOrEmpty(edit.FindControl<TextBlock>("error").Text));
        Assert.Equal("invalid", owner.Properties[0].Value);
        valueNode.Value = 15;
        var editGroupView = edit.FindControl<ItemsControl>("componentGroups").ItemTemplate.Build(editGroups[1]);
        editGroupView.DataContext = editGroups[1];
        var editRow = Assert.Single(editGroupView.GetVisualDescendants().OfType<ItemsControl>()).ItemTemplate.Build(valueRow);
        editRow.DataContext = valueRow;
        Assert.Empty(editRow.GetVisualDescendants().OfType<CheckBox>());
        planType.GetMethod("Apply").Invoke(plan, null);
        Assert.Equal(1, owner.Properties.Single(property => property.Name == "Health").Value);
        Assert.Equal(15, derived.Properties.Single(property => property.Name == "Health").Value);
        Assert.Equal(1, owner.OveriddenProperties.Single(property => property.Name == "Health").Value);
        Assert.Equal(20, child.Properties.Single(property => property.Name == "Health").Value);
        Assert.Equal(20, inheritedChild.Properties[0].Value);
        planType.GetMethod("Undo").Invoke(plan, null);
        Assert.Equal("20", inheritedChild.Properties[0].Value);

        var ownerOnlyPlan = Activator.CreateInstance(planType, new object[] { new[] { owner }, owner.FullName, original, replacement, typeof(int) });
        var ownerOnlyEdit = (Window)Activator.CreateInstance(dialogType, BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object[] { ownerOnlyPlan, new TestIntDefinition(), Array.Empty<string>(), true, false, false }, null);
        Assert.Empty(ownerOnlyEdit.FindControl<ItemsControl>("componentGroups").ItemsSource.Cast<object>());
        Assert.True(ownerOnlyEdit.FindControl<TextBlock>("noComponentReferences").IsVisible);
        planType.GetMethod("Apply").Invoke(ownerOnlyPlan, null);
        Assert.Equal(1, owner.Properties[0].Value);
        planType.GetMethod("Undo").Invoke(ownerOnlyPlan, null);

        var ownerOnlyRemovePlan = Activator.CreateInstance(removeType, new object[] { new[] { owner }, owner.FullName, "Health" });
        var ownerOnlyRemoval = (Window)Activator.CreateInstance(dialogType, BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object[] { "Game.Actor.Health", ownerOnlyRemovePlan, Array.Empty<string>() }, null);
        Assert.Empty(ownerOnlyRemoval.FindControl<ItemsControl>("componentGroups").ItemsSource.Cast<object>());
        Assert.True(ownerOnlyRemoval.FindControl<TextBlock>("noComponentReferences").IsVisible);
        removeType.GetMethod("Apply").Invoke(ownerOnlyRemovePlan, null);
        Assert.Empty(owner.Properties);
        removeType.GetMethod("Undo").Invoke(ownerOnlyRemovePlan, null);
        Assert.Equal("invalid", Assert.Single(owner.Properties).Value);
    }

    private static void VerifyCustomPropertyDialogLayout()
    {
        var theme = new FluentTheme();
        Application.Current.Styles.Add(theme);
        var resources = DispatchProxy.Create<IResourceModel, ResourceProxy>();
        var notifyType = typeof(Component).Assembly.GetType("TypeD.Models.UINotifyModel", throwOnError: true);
        ((ResourceProxy)resources).UINotify = (IUINotifyModel)Activator.CreateInstance(notifyType);
        var resourceProperty = typeof(ViewModelBase).GetProperty(nameof(ViewModelBase.ResourceModel));
        var previousResources = resourceProperty.GetValue(null);
        resourceProperty.SetValue(null, resources);
        try
        {
            var definitions = new IPropertyDefinition[] { new TestIntDefinition() };
            foreach (var existing in new CustomProperty?[]
            {
                null,
                new CustomProperty { Name = "Health", TypeName = typeof(int).FullName, DefaultValue = 10 }
            })
            {
                var dialog = new AddCustomPropertyDialog(definitions, Array.Empty<string>(), Array.Empty<Component>(), existing);
                var root = (Grid)dialog.Content;
                var fields = dialog.FindControl<ScrollViewer>("propertyFields");
                var footer = dialog.FindControl<Grid>("buttonPanel");
                var cancel = dialog.FindControl<Button>("cancelButton");
                var submit = dialog.FindControl<Button>("submitButton");
                foreach (var size in new[] { new Size(300, 220), new Size(450, 640) })
                {
                    root.Measure(size);
                    root.Arrange(new Rect(size));
                    Assert.True(fields.Bounds.Bottom <= footer.Bounds.Top);
                    Assert.True(footer.Bounds.Bottom <= root.Bounds.Height);
                    foreach (var button in new[] { cancel, submit })
                    {
                        Assert.Equal(HorizontalAlignment.Center, button.HorizontalContentAlignment);
                        Assert.Equal(VerticalAlignment.Center, button.VerticalContentAlignment);
                        var presenter = Assert.Single(button.GetVisualDescendants().OfType<ContentPresenter>());
                        var content = Assert.IsAssignableFrom<Control>(presenter.Child);
                        var origin = content.TranslatePoint(new Point(), button);
                        Assert.NotNull(origin);
                        Assert.InRange(Math.Abs(origin.Value.X + content.Bounds.Width / 2 - button.Bounds.Width / 2), 0, 1);
                        Assert.InRange(Math.Abs(origin.Value.Y + content.Bounds.Height / 2 - button.Bounds.Height / 2), 0, 1);
                    }
                    if (size.Height == 220)
                    {
                        var content = Assert.IsAssignableFrom<Control>(fields.Content);
                        content.Measure(new Size(fields.Bounds.Width, double.PositiveInfinity));
                        Assert.True(content.DesiredSize.Height > fields.Bounds.Height);
                        Assert.Equal(ScrollBarVisibility.Auto, fields.VerticalScrollBarVisibility);
                    }
                }
                if (existing == null)
                {
                    submit.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    var error = dialog.FindControl<TextBlock>("validationError");
                    Assert.True(error.IsVisible);
                    Assert.Contains("Enter a property name", error.Text);
                }
            }
        }
        finally
        {
            resourceProperty.SetValue(null, previousResources);
            Application.Current.Styles.Remove(theme);
        }
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

    [Fact]
    public void DrawStackComponent_CanOnlyBeAddedToASceneAndHoldsEntitiesAndDrawables()
    {
        var stack = CoreComponent.DrawStackComponent();
        Assert.Equal(DrawStackName, RootType(stack));
        Assert.True(new FilterHelperProbe(CoreComponent.SceneComponent()).Accepts(stack));
        Assert.False(new FilterHelperProbe(CoreComponent.GameComponent()).Accepts(stack));
        Assert.False(new FilterHelperProbe(CoreComponent.EntityComponent()).Accepts(stack));
        Assert.False(new FilterHelperProbe(CoreComponent.WindowComponent()).Accepts(stack));
        Assert.False(new FilterHelperProbe(stack).Accepts(stack));
        Assert.True(new FilterHelperProbe(stack).Accepts(CoreComponent.EntityComponent()));
        Assert.True(new FilterHelperProbe(stack).Accepts(CoreComponent.DrawableComponent()));
        Assert.False(new FilterHelperProbe(stack).Accepts(CoreComponent.SceneComponent()));
    }

    [Fact]
    public void SceneCode_DrawStackChildrenCreateTheirChildrenUnderTheStack()
    {
        var scene = Derive("Level", CoreComponent.SceneComponent());
        var rootEntity = Child("Root", CoreComponent.EntityComponent(), false);
        var stack = Child("Hud", CoreComponent.DrawStackComponent(), false);
        stack.Properties.Add(new Property { Name = "Name", Type = typeof(string), Value = "Hud" });
        stack.Properties.Add(new Property { Name = "DrawOrder", Type = typeof(int), Value = 5 });
        stack.Properties.Add(new Property { Name = "IgnoreCamera", Type = typeof(bool), Value = true });
        var stackEntity = Child("Button", CoreComponent.EntityComponent(), true);
        var stackDrawable = Child("Label", CoreComponent.DrawableComponent(), false);
        stack.Children.AddRange(new[] { stackEntity, stackDrawable });
        scene.Children.AddRange(new[] { rootEntity, stack });

        var output = GenerateSceneCode(scene);

        Assert.Contains("DrawAllStacks(Canvas);", output);
        Assert.Contains("this.CreateDrawStack(\"", output);
        Assert.Contains("\", \"Hud\", 5, true);", output);
        Assert.Contains("Entities.Create<Game.Button>(", output);
        Assert.Contains(", drawStack: child1);", output);
        Assert.Contains("child1.DrawableManager.Create<Game.Label>(", output);
        Assert.Contains("public Game.Button Button", output);
        Assert.DoesNotContain("Entities.Create<Game.Root>(\"" + rootEntity.ID + "\", drawStack", output);
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

    private static string GenerateSceneCode(Component scene)
    {
        var code = new TestSceneCode(scene);
        code.GenerateScene();
        return code.Output;
    }

    private sealed class TestSceneCode(Component component) : TypeDCore.Code.Scene.SceneCode(component)
    {
        public string Output => TypeDFile.Output.ToString();

        public void GenerateScene()
        {
            TypeDFile.AutoGeneratedFile = false;
            Writer.TargetFile = TypeDFile;
            InitTypeDClass();
            Generate();
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
        public IUINotifyModel UINotify { get; set; }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (targetMethod.Name == nameof(IResourceModel.Get))
            {
                var type = targetMethod.GetGenericArguments()[0];
                if (type == typeof(IComponentProvider)) return Provider;
                if (type == typeof(IUINotifyModel)) return UINotify;
                return null;
            }
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
