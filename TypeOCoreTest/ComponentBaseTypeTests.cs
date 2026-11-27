#nullable enable annotations

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Documents;
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
using TypeDCore.Models.Data.SettingContexts;
using TypeDCore.View.Dialogs.Project;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Attributes;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;
using TypeOEngine.Typedeaf.Core.Interfaces;

namespace TypeOCoreTest;

/// <summary>
/// Pins down how the root engine type of a component is used, so that the way it is determined can be changed safely.
/// </summary>
[Collection("EditorStaticResources")]
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
    public void AddProperty_RemembersFieldsWithoutNameAndDoesNotAffectEditing()
    {
        var settings = new PropertyCreationSettingContext();
        settings.TypeName.Value = typeof(int).FullName;
        settings.DefaultValueJson.Value = "42";
        settings.Description.Value = "Hit points";
        settings.Group.Value = "Stats";
        settings.Access.Value = CustomPropertyAccess.Protected;
        settings.RememberFields.Value = false;
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.ViewModel.Dialogs.Project.AddCustomPropertyViewModel", true);
        var definitions = new IPropertyDefinition[] { new TestIntDefinition() };
        object Create(CustomProperty existing) => Activator.CreateInstance(type, new object[]
            { null, definitions, Array.Empty<string>(), Array.Empty<Component>(), existing, settings });
        object Get(object view, string name) => type.GetProperty(name).GetValue(view);

        var firstUse = Activator.CreateInstance(type, new object[]
            { null, definitions, Array.Empty<string>(), Array.Empty<Component>(), null, new PropertyCreationSettingContext() });
        Assert.True((bool)Get(firstUse, "RememberFields"));
        Assert.NotNull(Get(firstUse, "SelectedType"));
        Assert.Null(Get(firstUse, "Error"));
        var disabled = Create(null);
        Assert.Equal("General", Get(disabled, "Group"));
        Assert.Equal("Public", Get(disabled, "SelectedAccess"));
        Assert.False((bool)Get(disabled, "RememberFields"));

        settings.RememberFields.Value = true;
        var restored = Create(null);
        Assert.Equal("", Get(restored, "PropertyName"));
        Assert.Equal("Hit points", Get(restored, "Description"));
        Assert.Equal("Stats", Get(restored, "Group"));
        Assert.Equal("Protected", Get(restored, "SelectedAccess"));
        type.GetProperty("PropertyName").SetValue(restored, "Health");
        var built = (CustomProperty)type.GetMethod("Build").Invoke(restored, null);
        Assert.Equal(typeof(int).FullName, built.TypeName);
        Assert.Equal(42, built.DefaultValue);
        var existing = new CustomProperty { Name = "Other", TypeName = typeof(int).FullName, DefaultValue = 7 };
        var edit = Create(existing);
        Assert.False((bool)Get(edit, "IsAdding"));
        Assert.Equal("Other", Get(edit, "PropertyName"));
        Assert.Equal(7, ((CustomProperty)type.GetMethod("Build").Invoke(edit, null)).DefaultValue);

        settings.TypeName.Value = "Unavailable.Type";
        var unavailable = Create(null);
        Assert.Null(Get(unavailable, "SelectedType"));
        Assert.Contains("unavailable", (string)Get(unavailable, "Error"));
    }

    [Fact]
    public void AddProperty_RestoresStructuredDefaultValueAsItsOriginalType()
    {
        var value = new Vec2(3.5, -2);
        var settings = new PropertyCreationSettingContext();
        settings.RememberFields.Value = true;
        settings.TypeName.Value = typeof(Vec2).FullName;
        settings.DefaultValueJson.Value = JsonSerializer.Serialize(value, JSON.CreateOptions());
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.ViewModel.Dialogs.Project.AddCustomPropertyViewModel", true);
        var view = Activator.CreateInstance(type, new object[]
        {
            null, new IPropertyDefinition[] { new TestValueDefinition<Vec2>() },
            Array.Empty<string>(), Array.Empty<Component>(), null, settings
        });
        type.GetProperty("PropertyName").SetValue(view, "Position");
        var property = (CustomProperty)type.GetMethod("Build").Invoke(view, null);
        Assert.Equal(value, Assert.IsType<Vec2>(property.DefaultValue));
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

    [Theory]
    [InlineData(CustomPropertyAccess.Private)]
    [InlineData(CustomPropertyAccess.Protected)]
    public void EditCustomProperty_MakingPropertyPublicAddsItToDerivedComponentsAndUndoRemovesIt(CustomPropertyAccess access)
    {
        var owner = new Component { Namespace = "Game", ClassName = "Actor" };
        var original = new CustomProperty
        {
            Name = "Health", TypeName = typeof(int).FullName, DefaultValue = 10, Access = access
        };
        owner.CustomProperties.Add(original);
        var derived = new Component { Namespace = "Game", ClassName = "Enemy", BaseInheritedComponent = owner };
        var replacement = new CustomProperty(original) { Access = CustomPropertyAccess.Public };
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.EditCustomPropertyPlan", true);
        var plan = Activator.CreateInstance(type, new object[] { new[] { owner, derived }, owner.FullName, original, replacement, typeof(int) });

        type.GetMethod("Apply").Invoke(plan, null);
        Assert.Equal(10, Assert.Single(owner.Properties).Value);
        var inherited = Assert.Single(derived.Properties);
        Assert.Equal(10, inherited.Value);
        Assert.Same(owner, inherited.FromComponent);
        Assert.Equal(10, Assert.Single(derived.OveriddenProperties).Value);
        type.GetMethod("Undo").Invoke(plan, null);
        Assert.Empty(owner.Properties);
        Assert.Empty(derived.Properties);
        Assert.Equal(access, original.Access);
        type.GetMethod("Apply").Invoke(plan, null);
        Assert.Single(derived.Properties);
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

    [Fact]
    public void ExtractProperties_UsesEditedInheritanceBeforeRebuild()
    {
        Component Of(Type type, Component parent = null) => new()
        {
            Namespace = type.Namespace,
            ClassName = type.FullName.Substring(type.Namespace.Length + 1),
            BaseInheritedComponent = parent
        };
        var engine = Of(typeof(TypeOEngine.Typedeaf.Core.Entities.Entity));
        var replacement = Of(typeof(ReplacementBase), engine);
        var component = Of(typeof(CompiledDerived), replacement);
        var initializerType = typeof(CoreComponent).Assembly.GetType("TypeDCore.TypeDCoreInitializer", true);
        var initializer = Activator.CreateInstance(initializerType, nonPublic: true);
        var modelType = typeof(TypeDInit).Assembly.GetType("TypeD.Models.ComponentModel", true);
        var propertyModelType = typeof(TypeDInit).Assembly.GetType("TypeD.Models.PropertyModel", true);
        initializerType.GetProperty("ComponentModel", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(initializer, Activator.CreateInstance(modelType, nonPublic: true));
        initializerType.GetProperty("PropertyModel", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(initializer, Activator.CreateInstance(propertyModelType, nonPublic: true));
        var provider = DispatchProxy.Create<IComponentProvider, ComponentCreationTests.CallbackProxy>();
        ((ComponentCreationTests.CallbackProxy)provider).Callback = (method, args) => null;
        initializerType.GetProperty("ComponentProvider", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(initializer, provider);
        var extract = initializerType.GetMethod("ExtractProperties");

        var hook = new ExtractPropertiesHook(component);
        extract.Invoke(initializer, new object[] { hook });
        Assert.Contains(hook.Properties, property => property.Name == "Own");
        Assert.Contains(hook.Properties, property => property.Name == "Replacement" && property.Type == typeof(string));
        Assert.DoesNotContain(hook.Properties, property => property.Name == "Former");

        component.ClassName = "NotYetCompiled";
        hook = new ExtractPropertiesHook(component);
        extract.Invoke(initializer, new object[] { hook });
        Assert.Contains(hook.Properties, property => property.Name == "Replacement");
        Assert.DoesNotContain(hook.Properties, property => property.Name == "Former");
    }

    public class FormerBase : TypeOEngine.Typedeaf.Core.Entities.Entity
    {
        [TypeOProperty]
        public int Former { get; set; }
    }

    public class ReplacementBase : TypeOEngine.Typedeaf.Core.Entities.Entity
    {
        [TypeOProperty]
        public string Replacement { get; set; } = "replacement";
    }

    public class CompiledDerived : FormerBase
    {
        [TypeOProperty]
        public int Own { get; set; }
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

    private sealed class TestValueDefinition<T> : IPropertyDefinition
    {
        public Type Type => typeof(T);
        public Type ValueType => typeof(T);
        public Type PropertyTemplateType => typeof(T);
        public JsonConverter JsonConverter => null;
        public IDataTemplate CreateTemplate() => null;
        public string ToCode(object value) => value.ToString();
        public PropertyNode CreateNode(Component component, Property property, Action<PropertyNode, object> valueChanged)
            => new PropertyNode(component, property, valueChanged);
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
                var rememberProperty = dialog.FindControl<CheckBox>("rememberFields");
                Assert.Equal(existing == null, rememberProperty.IsVisible);
                if (existing == null)
                {
                    Assert.True(rememberProperty.IsChecked);
                    rememberProperty.IsChecked = false;
                    Assert.Equal(false, dialog.DataContext.GetType().GetProperty("RememberFields").GetValue(dialog.DataContext));
                }
                var cancel = dialog.FindControl<Button>("cancelButton");
                var submit = dialog.FindControl<Button>("submitButton");
                foreach (var size in new[] { new Size(300, 220), new Size(450, 640) })
                {
                    root.Measure(size);
                    root.Arrange(new Rect(size));
                    Assert.True(fields.Bounds.Bottom <= footer.Bounds.Top);
                    Assert.True(footer.Bounds.Bottom <= root.Bounds.Height);
                    if (existing == null)
                    {
                        Assert.True(rememberProperty.Bounds.Right <= cancel.Bounds.Left);
                        Assert.True(rememberProperty.Bounds.Top < cancel.Bounds.Bottom && cancel.Bounds.Top < rememberProperty.Bounds.Bottom);
                        var label = Assert.IsType<AccessText>(rememberProperty.Content);
                        var labelOrigin = label.TranslatePoint(new Point(), footer);
                        Assert.NotNull(labelOrigin);
                        Assert.True(labelOrigin.Value.X + label.Bounds.Width <= cancel.Bounds.Left);
                    }
                    Assert.True(cancel.Bounds.Right <= submit.Bounds.Left);
                    Assert.True(submit.Bounds.Right <= footer.Bounds.Width);
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

                var project = ComponentCreationTests.MakeProject();
                var entity2d = new Component
                {
                    Namespace = "Engine", ClassName = "Entity2d", BaseInheritedComponent = CoreComponent.EntityComponent()
                };
                var otherEntity2d = new Component
                {
                    Namespace = "Game", ClassName = "Entity2d", BaseInheritedComponent = CoreComponent.EntityComponent()
                };
                entity2d.CustomProperties.Add(new CustomProperty { Name = "Inherited", TypeName = typeof(int).FullName });
                var createDialog = new CreateComponentTypeDialog(project, null, CoreComponent.EntityComponent(),
                    new[] { entity2d, otherEntity2d }, new ComponentCreationSettingContext());
                var createRoot = (Grid)createDialog.Content;
                var rememberComponent = createDialog.FindControl<CheckBox>("RememberFields");
                Assert.True(rememberComponent.IsChecked);
                rememberComponent.IsChecked = false;
                Assert.Equal(false, createDialog.DataContext.GetType().GetProperty("RememberFields").GetValue(createDialog.DataContext));
                var componentFields = createDialog.FindControl<ScrollViewer>("ComponentFields");
                var creationFooter = createDialog.FindControl<Grid>("CreationFooter");
                var cancelComponent = createDialog.FindControl<Button>("CancelButton");
                var createButton = createDialog.FindControl<Button>("CreateButton");
                var nameField = createDialog.FindControl<TextBox>("ComponentName");
                var namespaceLabel = createDialog.FindControl<Label>("NamespaceLabel");
                var namespaceRow = createDialog.FindControl<Grid>("NamespaceRow");
                var namespaceError = createDialog.FindControl<TextBlock>("NamespaceError");
                var baseTypeLabel = createDialog.FindControl<Label>("BaseTypeLabel");
                Assert.False(namespaceError.IsVisible);
                var propertyList = createDialog.FindControl<ListBox>("CustomProperties");
                var editProperty = createDialog.FindControl<Button>("EditCustomProperty");
                var removeProperty = createDialog.FindControl<Button>("RemoveCustomProperty");
                var view = createDialog.DataContext;
                var draft = new CustomProperty { Name = "Health", TypeName = typeof(int).FullName, DefaultValue = 10 };
                view.GetType().GetMethod("AddProperty").Invoke(view, new object[] { draft, typeof(int), true });
                Assert.False(editProperty.IsEnabled);
                propertyList.SelectedItem = Assert.Single(propertyList.ItemsSource.Cast<object>());
                Assert.True(editProperty.IsEnabled);
                Assert.True(removeProperty.IsEnabled);
                foreach (var size in new[] { new Size(320, 280), new Size(560, 620) })
                {
                    createRoot.Measure(size);
                    createRoot.Arrange(new Rect(size));
                    Assert.True(componentFields.Bounds.Bottom <= creationFooter.Bounds.Top);
                    Assert.True(creationFooter.Bounds.Bottom <= createRoot.Bounds.Height);
                    Assert.True(rememberComponent.Bounds.Right <= cancelComponent.Bounds.Left);
                    Assert.True(rememberComponent.Bounds.Top < cancelComponent.Bounds.Bottom && cancelComponent.Bounds.Top < rememberComponent.Bounds.Bottom);
                    var label = Assert.IsType<AccessText>(rememberComponent.Content);
                    var labelOrigin = label.TranslatePoint(new Point(), creationFooter);
                    Assert.NotNull(labelOrigin);
                    Assert.True(labelOrigin.Value.X + label.Bounds.Width <= cancelComponent.Bounds.Left);
                    Assert.True(cancelComponent.Bounds.Right <= createButton.Bounds.Left);
                    Assert.True(createButton.Bounds.Right <= creationFooter.Bounds.Width);
                    var fieldsContent = Assert.IsAssignableFrom<Control>(componentFields.Content);
                    fieldsContent.Measure(new Size(componentFields.Bounds.Width, double.PositiveInfinity));
                    fieldsContent.Arrange(new Rect(0, 0, componentFields.Bounds.Width, fieldsContent.DesiredSize.Height));
                    Assert.Equal(namespaceLabel.Bounds.Top - nameField.Bounds.Bottom,
                        baseTypeLabel.Bounds.Top - namespaceRow.Bounds.Bottom, 3);
                    var scrollbar = Assert.Single(componentFields.GetVisualDescendants().OfType<ScrollBar>(),
                        bar => bar.Orientation == Orientation.Vertical &&
                            bar.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault() == componentFields);
                    var scrollbarOrigin = scrollbar.TranslatePoint(new Point(), componentFields);
                    Assert.NotNull(scrollbarOrigin);
                    Assert.True(nameField.Bounds.Width > 0);
                    Assert.True(scrollbar.Bounds.Width > 0);
                    Assert.True(fieldsContent.Bounds.X + nameField.Bounds.Right + 4 <= scrollbarOrigin.Value.X);
                }
                namespaceError.DataContext = new { NamespaceSelectionError = "Choose a folder inside the project source folder." };
                Assert.True(namespaceError.IsVisible);
                Assert.Equal("Choose a folder inside the project source folder.", namespaceError.Text);
                namespaceError.ClearValue(Control.DataContextProperty);
                var selector = createDialog.FindControl<ComboBox>("RootType");
                var entityOptions = createDialog.FindControl<StackPanel>("EntityOptions");
                Assert.True(entityOptions.IsVisible);
                Assert.Collection(entityOptions.Children.OfType<CheckBox>(),
                    checkBox => Assert.True(checkBox.IsChecked),
                    checkBox => Assert.True(checkBox.IsChecked));
                createDialog.FindControl<TextBox>("ComponentName").Text = "Actor";
                Assert.Null(createDialog.FindControl<TextBlock>("ResultName"));
                createDialog.FindControl<AutoCompleteBox>("ComponentNamespace").Text = "Game.Characters";
                var baseBox = createDialog.FindControl<AutoCompleteBox>("ParentComponent");
                Assert.Contains(entity2d.FullName, baseBox.ItemsSource.Cast<string>());
                Assert.Equal(CoreComponent.EntityComponent().ClassName, baseBox.Text);
                baseBox.IsDropDownOpen = true;
                baseBox.SelectedItem = entity2d.FullName;
                Assert.Equal(entity2d.FullName, baseBox.Text);
                baseBox.IsDropDownOpen = false;
                Assert.Equal(entity2d.ClassName, baseBox.Text);
                Assert.Same(entity2d, createDialog.DataContext.GetType().GetProperty("ParentComponent").GetValue(createDialog.DataContext));
                var inheritedRow = propertyList.ItemsSource.Cast<object>().First();
                var propertyContent = Assert.IsType<StackPanel>(propertyList.ItemTemplate.Build(inheritedRow));
                propertyContent.DataContext = inheritedRow;
                var header = Assert.IsType<TextBlock>(propertyContent.Children[0]);
                Assert.Collection(header.Inlines.OfType<Run>(),
                    name => Assert.Equal("Inherited", name.Text),
                    inherited => Assert.Equal(" (inherited from Engine.Entity2d)", inherited.Text));
                Assert.DoesNotContain("inherited", Assert.IsType<TextBlock>(propertyContent.Children[1]).Text);
                propertyList.SelectedItem = inheritedRow;
                Assert.False(editProperty.IsEnabled);
                Assert.False(removeProperty.IsEnabled);
                baseBox.IsDropDownOpen = true;
                baseBox.SelectedItem = otherEntity2d.FullName;
                baseBox.IsDropDownOpen = false;
                Assert.Equal(otherEntity2d.ClassName, baseBox.Text);
                Assert.Same(otherEntity2d, createDialog.DataContext.GetType().GetProperty("ParentComponent").GetValue(createDialog.DataContext));
                var dialogModel = DispatchProxy.Create<IEditorDialogModel, ComponentCreationTests.CallbackProxy>();
                var chosenParent = new Component
                {
                    Namespace = "Game", ClassName = "Actor", BaseInheritedComponent = CoreComponent.EntityComponent()
                };
                ((ComponentCreationTests.CallbackProxy)dialogModel).Callback = (method, args) =>
                {
                    if (method.Name == nameof(IEditorDialogModel.SelectIconAsync))
                        return Task.FromResult("custom.png");
                    var filter = (FilterHelper)args[1];
                    Assert.False(filter.Filter(chosenParent.RootBaseComponent.FullName));
                    Assert.True(filter.Filter(chosenParent.RootBaseComponent.FullName + "Extra"));
                    Assert.True(filter.Filter(CoreComponent.SceneComponent().FullName));
                    return Task.FromResult(chosenParent);
                };
                ((ResourceProxy)resources).Dialogs = dialogModel;
                var creationViewModel = createDialog.DataContext;
                ((Task)creationViewModel.GetType().GetMethod("OpenComponents").Invoke(creationViewModel, null)).GetAwaiter().GetResult();
                Assert.Equal(chosenParent.ClassName, createDialog.FindControl<AutoCompleteBox>("ParentComponent").Text);
                ((Task)creationViewModel.GetType().GetMethod("OpenIcons").Invoke(creationViewModel, null)).GetAwaiter().GetResult();
                Assert.Equal("custom.png", creationViewModel.GetType().GetProperty("IconName").GetValue(creationViewModel));
                creationViewModel.GetType().GetMethod("InheritIcon").Invoke(creationViewModel, null);
                Assert.Null(creationViewModel.GetType().GetProperty("IconName").GetValue(creationViewModel));
                selector.SelectedIndex = 1;
                Assert.False(entityOptions.IsVisible);
                Assert.Equal(CoreComponent.SceneComponent().ClassName, createDialog.FindControl<AutoCompleteBox>("ParentComponent").Text);
                Assert.Equal("Game.Characters", createDialog.FindControl<AutoCompleteBox>("ComponentNamespace").Text);

                chosenParent.CustomProperties.Add(new CustomProperty
                    { Name = "Existing", TypeName = typeof(int).FullName, DefaultValue = 5 });
                var editDialog = (CreateComponentTypeDialog)Activator.CreateInstance(typeof(CreateComponentTypeDialog),
                    BindingFlags.Instance | BindingFlags.NonPublic, null,
                    new object[] { project, chosenParent, Array.Empty<Component>() }, null);
                Assert.True(editDialog.FindControl<ComboBox>("RootType").IsEnabled);
                Assert.True(editDialog.FindControl<TextBox>("ComponentName").IsEnabled);
                Assert.True(editDialog.FindControl<Grid>("NamespaceRow").IsEnabled);
                Assert.True(editDialog.FindControl<Grid>("BaseTypeRow").IsEnabled);
                Assert.False(editDialog.FindControl<CheckBox>("RememberFields").IsVisible);
                var editList = editDialog.FindControl<ListBox>("CustomProperties");
                var editButton = editDialog.FindControl<Button>("EditCustomProperty");
                editList.SelectedItem = Assert.Single(editList.ItemsSource.Cast<object>());
                Assert.True(editButton.IsEnabled);
                var editView = editDialog.DataContext;
                var newProperty = new CustomProperty { Name = "New", TypeName = typeof(int).FullName, DefaultValue = 7 };
                editView.GetType().GetMethod("AddProperty").Invoke(editView, new object[] { newProperty, typeof(int), false });
                editList.SelectedItem = editList.ItemsSource.Cast<object>().Single(row =>
                    ReferenceEquals(row.GetType().GetProperty("Property").GetValue(row), newProperty));
                Assert.True(editButton.IsEnabled);

                var inheritedCapabilities = new Component
                {
                    Namespace = "Game", ClassName = "InheritedBase", BaseInheritedComponent = CoreComponent.EntityComponent()
                };
                inheritedCapabilities.Interfaces.Add(typeof(IUpdatable));
                var inheritedEdit = new Component
                {
                    Namespace = "Game", ClassName = "InheritedActor", BaseInheritedComponent = inheritedCapabilities
                };
                var inheritedDialog = (CreateComponentTypeDialog)Activator.CreateInstance(typeof(CreateComponentTypeDialog),
                    BindingFlags.Instance | BindingFlags.NonPublic, null,
                    new object[] { project, inheritedEdit, Array.Empty<Component>() }, null);
                var inheritedChecks = inheritedDialog.FindControl<StackPanel>("EntityOptions").Children.OfType<CheckBox>().ToArray();
                Assert.True(inheritedChecks[0].IsChecked);
                Assert.False(inheritedChecks[0].IsEnabled);
                Assert.False(inheritedChecks[1].IsChecked);
                Assert.True(inheritedChecks[1].IsEnabled);

                if (existing == null)
                {
                    VerifyEditComponentCommand(project, ((ResourceProxy)resources).UINotify);
                    VerifyIdentityEditCommand(((ResourceProxy)resources).UINotify);
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

    private static void VerifyIdentityEditCommand(IUINotifyModel notify)
    {
        var directory = Path.Combine(Path.GetTempPath(), "TypeOIdentityEdit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var project = ComponentCreationTests.MakeProject();
        typeof(Project).GetProperty(nameof(Project.Location)).SetValue(project, directory);
        project.StartScene = "Game.Actor";
        var root = CoreComponent.EntityComponent();
        var newBase = new Component
        {
            Namespace = "Game", ClassName = "NewBase", BaseInheritedComponent = root, Template = new EntityComponentTemplate()
        };
        var component = new Component
        {
            Namespace = "Game", ClassName = "Actor", BaseInheritedComponent = root, Template = new EntityComponentTemplate()
        };
        var derived = new Component
        {
            Namespace = "Game", ClassName = "Enemy", BaseInheritedComponent = component, Template = new EntityComponentTemplate()
        };
        var roots = new List<Component> { component, derived };
        var provider = DispatchProxy.Create<IComponentProvider, ComponentCreationTests.CallbackProxy>();
        string ComponentPath(string name) => Path.Combine(directory, "components",
            name.Replace('.', Path.DirectorySeparatorChar) + ".component");
        ((ComponentCreationTests.CallbackProxy)provider).Callback = (method, args) => method.Name switch
        {
            nameof(IComponentProvider.ListAll) => roots.ToList(),
            nameof(IComponentProvider.GetBaseTypeComponents) => new List<Component> { root, component, derived, newBase },
            nameof(IComponentProvider.GetPath) => ComponentPath(args[1] is Component item ? item.FullName : (string)args[1]),
            _ => throw new InvalidOperationException(method.Name)
        };
        var projects = DispatchProxy.Create<IProjectModel, ComponentCreationTests.CallbackProxy>();
        IResourceModel resources = null;
        ((ComponentCreationTests.CallbackProxy)projects).Callback = (method, args) =>
        {
            if (method.Name == nameof(IProjectModel.InitCode))
            {
                var code = (Codalyzer)args[1];
                typeof(Codalyzer).GetProperty(nameof(Codalyzer.Project)).SetValue(code, project);
                typeof(Codalyzer).GetProperty(nameof(Codalyzer.Resources)).SetValue(code, resources);
                code.Init();
            }
            return null;
        };
        var properties = DispatchProxy.Create<IPropertyModel, ComponentCreationTests.CallbackProxy>();
        ((ComponentCreationTests.CallbackProxy)properties).Callback = (method, args) =>
            method.Name == nameof(IPropertyModel.List) ? new IPropertyDefinition[] { new TestIntDefinition() } : null;
        var selection = DispatchProxy.Create<INodeSelectionModel, ComponentCreationTests.CallbackProxy>();
        ((ComponentCreationTests.CallbackProxy)selection).Callback = (method, args) =>
            method.Name == "get_Project" ? project : null;
        var registry = DispatchProxy.Create<IEditorCommandRegistry, ComponentCreationTests.CallbackProxy>();
        ((ComponentCreationTests.CallbackProxy)registry).Callback = (method, args) => null;
        var log = DispatchProxy.Create<ILogModel, ComponentCreationTests.CallbackProxy>();
        ((ComponentCreationTests.CallbackProxy)log).Callback = (method, args) => null;
        var settings = DispatchProxy.Create<ISettingModel, ComponentCreationTests.CallbackProxy>();
        ((ComponentCreationTests.CallbackProxy)settings).Callback = (method, args) =>
            method.Name == nameof(ISettingModel.GetContext) ? new PropertyCreationSettingContext() : null;
        var hooks = (IHookModel)Activator.CreateInstance(typeof(Component).Assembly.GetType("TypeD.Models.HookModel", true));
        var history = (ICommandHistoryModel)Activator.CreateInstance(typeof(Component).Assembly.GetType("TypeD.Models.CommandHistoryModel", true));
        var saves = (ISaveModel)Activator.CreateInstance(typeof(Component).Assembly.GetType("TypeD.Models.SaveModel", true));
        var formats = (IFormatVersionModel)Activator.CreateInstance(typeof(Component).Assembly.GetType("TypeD.Models.FormatVersionModel", true));
        resources = DispatchProxy.Create<IResourceModel, ComponentCreationTests.CallbackProxy>();
        ((ComponentCreationTests.CallbackProxy)resources).Callback = (method, args) =>
        {
            var service = method.GetGenericArguments()[0];
            if (service == typeof(IComponentProvider)) return provider;
            if (service == typeof(IProjectModel)) return projects;
            if (service == typeof(IPropertyModel)) return properties;
            if (service == typeof(IHookModel)) return hooks;
            if (service == typeof(ILogModel)) return log;
            if (service == typeof(ICommandHistoryModel)) return history;
            if (service == typeof(INodeSelectionModel)) return selection;
            if (service == typeof(IEditorCommandRegistry)) return registry;
            if (service == typeof(ISettingModel)) return settings;
            if (service == typeof(IUINotifyModel)) return notify;
            if (service == typeof(ISaveModel)) return saves;
            if (service == typeof(IFormatVersionModel)) return formats;
            throw new InvalidOperationException(service.Name);
        };
        saves.Init(resources);
        foreach (var item in roots.Append(newBase))
        {
            typeof(TypeD.Components.ComponentTemplate).GetProperty("Component").SetValue(item.Template, item);
            item.Template.Init();
            projects.InitCode(project, item.Template.Code);
        }
        var save = saves.GetSaveContext<TypeD.Models.Data.SaveContexts.ComponentSaveContext>(project);
        foreach (var item in roots)
        {
            var path = ComponentPath(item.FullName);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonSerializer.Serialize(save.CreateDocument(item)));
            var generated = item.Template.Code.FilePathTypeD();
            Directory.CreateDirectory(Path.GetDirectoryName(generated));
            File.WriteAllText(generated, "old generated " + item.FullName);
        }
        var oldSource = Path.Combine(directory, "Game", "Actor.cs");
        var newSource = Path.Combine(directory, "Game", "Characters", "Hero.cs");
        Directory.CreateDirectory(Path.GetDirectoryName(oldSource));
        const string oldText = "namespace Game { public partial class Actor {} }";
        const string newText = "namespace Game.Characters { public partial class Hero {} }";
        File.WriteAllText(oldSource, oldText);
        File.WriteAllText(project.ProjectFilePath, "{\"StartScene\":\"Game.Actor\"}");
        var originalResource = typeof(ViewModelBase).GetProperty(nameof(ViewModelBase.ResourceModel)).GetValue(null);
        typeof(ViewModelBase).GetProperty(nameof(ViewModelBase.ResourceModel)).SetValue(null, resources);
        try
        {
            var assembly = typeof(CoreComponent).Assembly;
            var renameType = assembly.GetType("TypeDCore.Commands.PropertyCodeRename", true);
            var fileType = renameType.GetNestedType("FileChange", BindingFlags.NonPublic);
            var changes = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(fileType));
            changes.Add(Activator.CreateInstance(fileType, new object[] { oldSource, Encoding.UTF8.GetBytes(oldText), Encoding.UTF8.GetBytes(newText) }));
            var rename = Activator.CreateInstance(renameType, BindingFlags.Instance | BindingFlags.NonPublic,
                null, new object[] { changes }, null);
            var commandType = assembly.GetType("TypeDCore.Commands.EditComponentTypeCommand", true);
            var command = Activator.CreateInstance(commandType, new object[] { resources });
            var dataType = assembly.GetType("TypeDCore.Commands.Data.ComponentCommandData", true);
            var data = Activator.CreateInstance(dataType);
            dataType.GetProperty("Project").SetValue(data, project);
            dataType.GetProperty("Component").SetValue(data, component);
            var execute = commandType.GetMethod("ExecuteWithScanAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            var delegateType = execute.GetParameters()[3].ParameterType;
            var parameters = delegateType.GetMethod("Invoke").GetParameters()
                .Select(parameter => Expression.Parameter(parameter.ParameterType)).ToArray();
            var task = typeof(Task).GetMethod(nameof(Task.FromResult)).MakeGenericMethod(renameType).Invoke(null, new[] { rename });
            var scanner = Expression.Lambda(delegateType, Expression.Constant(task), parameters).Compile();
            Func<CreateComponentTypeDialog, Task<bool>> edit = dialog =>
            {
                var view = dialog.DataContext;
                view.GetType().GetProperty("ComponentName").SetValue(view, "Hero");
                view.GetType().GetProperty("ComponentNamespace").SetValue(view, "Game.Characters");
                view.GetType().GetMethod("OnParentComponentSet").Invoke(view, new object[] { newBase });
                view.GetType().GetMethod("AddProperty").Invoke(view, new object[]
                {
                    new CustomProperty { Name = "Health", TypeName = typeof(int).FullName, DefaultValue = 5 },
                    typeof(int), false
                });
                return Task.FromResult(true);
            };
            Func<string, Task<bool>> reject = text => Task.FromResult(false);
            Func<string, Task<bool>> accept = text =>
            {
                Assert.Contains("Game.Actor -> Game.Characters.Hero", text);
                return Task.FromResult(true);
            };
            void Run(Func<string, Task<bool>> confirm) =>
                ((Task)execute.Invoke(command, new object[] { data, edit, confirm, scanner })).GetAwaiter().GetResult();

            Run(reject);
            Assert.Equal("Game.Actor", component.FullName);
            Assert.Equal(oldText, File.ReadAllText(oldSource));
            Directory.CreateDirectory(Path.GetDirectoryName(newSource));
            File.WriteAllText(newSource, "existing");
            Assert.Throws<IOException>(() => Run(accept));
            Assert.Equal("existing", File.ReadAllText(newSource));
            Assert.Equal("Game.Actor", component.FullName);
            File.Delete(newSource);
            Run(accept);
            Assert.Equal("Game.Characters.Hero", component.FullName);
            Assert.Same(newBase, component.BaseInheritedComponent);
            Assert.Equal(component.FullName, project.StartScene);
            Assert.False(File.Exists(oldSource));
            Assert.Equal(newText, File.ReadAllText(newSource));
            Assert.False(File.Exists(ComponentPath("Game.Actor")));
            Assert.True(File.Exists(ComponentPath(component.FullName)));
            Assert.Contains("Game.Characters.Hero", File.ReadAllText(ComponentPath("Game.Enemy")));
            Assert.Contains("class Hero", File.ReadAllText(component.Template.Code.FilePathTypeD()));
            Assert.Contains("Health", File.ReadAllText(component.Template.Code.FilePathTypeD()));
            Assert.Single(component.CustomProperties);
            Assert.Contains("using Game.Characters;", File.ReadAllText(derived.Template.Code.FilePathTypeD()));
            Assert.Contains("class Enemy : Hero", File.ReadAllText(derived.Template.Code.FilePathTypeD()));
            Assert.Contains("Game.Characters.Hero", File.ReadAllText(project.ProjectFilePath));
            saves.AddSave<IdentityPendingSave>();
            Assert.Throws<InvalidOperationException>(() => history.Undo());
            saves.Save(null).GetAwaiter().GetResult();
            File.AppendAllText(newSource, " external");
            Assert.Throws<IOException>(() => history.Undo());
            Assert.Equal("Game.Characters.Hero", component.FullName);
            File.WriteAllText(newSource, newText);
            Assert.True(history.Undo());
            Assert.Equal("Game.Actor", component.FullName);
            Assert.Same(root, component.BaseInheritedComponent);
            Assert.Equal("Game.Actor", project.StartScene);
            Assert.Empty(component.CustomProperties);
            Assert.Equal(oldText, File.ReadAllText(oldSource));
            Assert.False(File.Exists(newSource));
            Assert.True(history.Redo());
            Assert.Equal(newText, File.ReadAllText(newSource));
            Assert.Same(newBase, component.BaseInheritedComponent);
            Assert.Single(component.CustomProperties);
        }
        finally
        {
            typeof(ViewModelBase).GetProperty(nameof(ViewModelBase.ResourceModel)).SetValue(null, originalResource);
            var saveKey = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                Encoding.UTF8.GetBytes(Path.GetFullPath(project.ProjectFilePath))));
            var statePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Typedeaf", "TypeO", "SaveState", $"{saveKey}.txt");
            if (File.Exists(statePath))
                File.Delete(statePath);
            Directory.Delete(directory, recursive: true);
        }
    }

    public sealed class IdentityPendingSave : SaveContext
    {
        public override void Init(IResourceModel resources, object param = null) { }
        public override Task SaveAction(Project project) => Task.CompletedTask;
    }

    private static void VerifyEditComponentCommand(Project project, IUINotifyModel notify)
    {
        var root = CoreComponent.EntityComponent();
        var oldBase = new Component
        {
            Namespace = "Game", ClassName = "OldBase", BaseInheritedComponent = root, Template = new EntityComponentTemplate()
        };
        oldBase.Properties.Add(new Property { Name = "Legacy", Type = typeof(int), Value = 1, DeclaringTypeName = oldBase.FullName });
        var newBase = new Component
        {
            Namespace = "Game", ClassName = "NewBase", BaseInheritedComponent = root, Template = new EntityComponentTemplate()
        };
        var inheritedName = new Property
        {
            Name = "Name", Type = typeof(string), Value = "default",
            DeclaringTypeName = typeof(TypeOEngine.Typedeaf.Core.Engine.TypeOObject).FullName
        };
        oldBase.Properties.Add(inheritedName);
        newBase.Properties.Add(new Property(inheritedName));
        var component = new Component
        {
            Namespace = "Game", ClassName = "Actor", BaseInheritedComponent = oldBase, Icon = "old.png"
        };
        component.Properties.Add(new Property(oldBase.Properties[0]) { Value = 9 });
        component.OveriddenProperties.Add(new Property(oldBase.Properties[0]));
        component.Properties.Add(new Property(inheritedName) { Value = "Actor name" });
        component.OveriddenProperties.Add(new Property(inheritedName));
        var template = new EntityComponentTemplate();
        typeof(TypeD.Components.ComponentTemplate).GetProperty("Component").SetValue(template, component);
        template.Init();
        component.Template = template;
        var saves = 0;
        var provider = DispatchProxy.Create<IComponentProvider, ComponentCreationTests.CallbackProxy>();
        ((ComponentCreationTests.CallbackProxy)provider).Callback = (method, args) => method.Name switch
        {
            nameof(IComponentProvider.ListAll) => new List<Component> { component },
            nameof(IComponentProvider.GetBaseTypeComponents) => new List<Component> { root, oldBase, newBase },
            nameof(IComponentProvider.GetPath) => Path.Combine(project.Location, "Actor.component"),
            nameof(IComponentProvider.Save) => CountSave(),
            _ => throw new InvalidOperationException(method.Name)
        };
        object CountSave() { saves++; return null; }
        var projects = DispatchProxy.Create<IProjectModel, ComponentCreationTests.CallbackProxy>();
        ((ComponentCreationTests.CallbackProxy)projects).Callback = (method, args) => null;
        var properties = DispatchProxy.Create<IPropertyModel, ComponentCreationTests.CallbackProxy>();
        ((ComponentCreationTests.CallbackProxy)properties).Callback = (method, args) =>
            method.Name == nameof(IPropertyModel.List) ? new IPropertyDefinition[] { new TestIntDefinition() } : null;
        var selection = DispatchProxy.Create<INodeSelectionModel, ComponentCreationTests.CallbackProxy>();
        ((ComponentCreationTests.CallbackProxy)selection).Callback = (method, args) =>
            method.Name == "get_Project" ? project : null;
        var settings = DispatchProxy.Create<ISettingModel, ComponentCreationTests.CallbackProxy>();
        ((ComponentCreationTests.CallbackProxy)settings).Callback = (method, args) =>
            method.Name == nameof(ISettingModel.GetContext) ? new PropertyCreationSettingContext() : null;
        var registry = DispatchProxy.Create<IEditorCommandRegistry, ComponentCreationTests.CallbackProxy>();
        ((ComponentCreationTests.CallbackProxy)registry).Callback = (method, args) => null;
        var log = DispatchProxy.Create<ILogModel, ComponentCreationTests.CallbackProxy>();
        ((ComponentCreationTests.CallbackProxy)log).Callback = (method, args) => null;
        var hooks = (IHookModel)Activator.CreateInstance(typeof(Component).Assembly.GetType("TypeD.Models.HookModel", true));
        var history = (ICommandHistoryModel)Activator.CreateInstance(typeof(Component).Assembly.GetType("TypeD.Models.CommandHistoryModel", true));
        var resources = DispatchProxy.Create<IResourceModel, ComponentCreationTests.CallbackProxy>();
        ((ComponentCreationTests.CallbackProxy)resources).Callback = (method, args) =>
        {
            var service = method.GetGenericArguments()[0];
            if (service == typeof(IComponentProvider)) return provider;
            if (service == typeof(IProjectModel)) return projects;
            if (service == typeof(IPropertyModel)) return properties;
            if (service == typeof(IHookModel)) return hooks;
            if (service == typeof(ILogModel)) return log;
            if (service == typeof(ICommandHistoryModel)) return history;
            if (service == typeof(INodeSelectionModel)) return selection;
            if (service == typeof(IEditorCommandRegistry)) return registry;
            if (service == typeof(ISettingModel)) return settings;
            if (service == typeof(IUINotifyModel)) return notify;
            throw new InvalidOperationException(service.Name);
        };
        var resourceProperty = typeof(ViewModelBase).GetProperty(nameof(ViewModelBase.ResourceModel));
        var previous = resourceProperty.GetValue(null);
        resourceProperty.SetValue(null, resources);
        try
        {
            var assembly = typeof(CoreComponent).Assembly;
            var commandType = assembly.GetType("TypeDCore.Commands.EditComponentTypeCommand", true);
            var command = Activator.CreateInstance(commandType, new object[] { resources });
            var dataType = assembly.GetType("TypeDCore.Commands.Data.ComponentCommandData", true);
            var data = Activator.CreateInstance(dataType);
            dataType.GetProperty("Project").SetValue(data, project);
            var staleBrowserCopy = new Component
            {
                Namespace = component.Namespace, ClassName = component.ClassName,
                BaseInheritedComponent = oldBase, Icon = component.Icon
            };
            dataType.GetProperty("Component").SetValue(data, staleBrowserCopy);
            var execute = commandType.GetMethod("ExecuteWithDialogsAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Task Run(bool acceptDialog, bool acceptPreview)
            {
                Func<CreateComponentTypeDialog, Task<bool>> edit = dialog =>
                {
                    var view = dialog.DataContext;
                    view.GetType().GetProperty("IconName").SetValue(view, "new.png");
                    view.GetType().GetMethod("AddProperty").Invoke(view, new object[]
                    {
                        new CustomProperty { Name = "Health", TypeName = typeof(int).FullName, DefaultValue = 5 },
                        typeof(int), false
                    });
                    return Task.FromResult(acceptDialog);
                };
                Func<string, Task<bool>> confirm = text =>
                {
                    Assert.Contains("Health", text);
                    Assert.Contains("Actor.component", text);
                    return Task.FromResult(acceptPreview);
                };
                return (Task)execute.Invoke(command, new object[] { data, edit, confirm });
            }

            Run(false, true).GetAwaiter().GetResult();
            Run(true, false).GetAwaiter().GetResult();
            Assert.Empty(component.CustomProperties);
            Assert.Equal("old.png", component.Icon);
            Assert.Equal(0, saves);
            Run(true, true).GetAwaiter().GetResult();
            Assert.Equal("new.png", component.Icon);
            Assert.Single(component.CustomProperties);
            Assert.Empty(staleBrowserCopy.CustomProperties);
            Assert.True(history.Undo());
            Assert.Equal("old.png", component.Icon);
            Assert.Empty(component.CustomProperties);
            Assert.True(history.Redo());
            Assert.Equal("new.png", component.Icon);
            Assert.Single(component.CustomProperties);
            Assert.Equal(3, saves);

            Func<CreateComponentTypeDialog, Task<bool>> rebaseEditor = dialog =>
            {
                var view = dialog.DataContext;
                view.GetType().GetMethod("OnParentComponentSet").Invoke(view, new object[] { newBase });
                var existing = ((IReadOnlyList<CustomProperty>)view.GetType().GetProperty("CustomProperties").GetValue(view)).Single();
                view.GetType().GetMethod("ReplaceProperty").Invoke(view, new object[]
                {
                    existing, new CustomProperty(existing) { Access = CustomPropertyAccess.Protected }, typeof(int)
                });
                return Task.FromResult(true);
            };
            Func<string, Task<bool>> confirmRebase = text =>
            {
                Assert.Contains("Legacy", text);
                Assert.Contains(newBase.FullName, text);
                return Task.FromResult(true);
            };
            ((Task)execute.Invoke(command, new object[] { data, rebaseEditor, confirmRebase })).GetAwaiter().GetResult();
            Assert.Same(newBase, component.BaseInheritedComponent);
            Assert.DoesNotContain(component.Properties, property => property.Name == "Legacy");
            Assert.DoesNotContain(component.Properties, property => property.Name == "Health");
            Assert.Equal("Actor name", component.Properties.Single(property => property.Name == "Name").Value);
            Assert.True(history.Undo());
            Assert.Same(oldBase, component.BaseInheritedComponent);
            Assert.Equal(9, component.Properties.Single(property => property.Name == "Legacy").Value);
            Assert.Equal("Actor name", component.Properties.Single(property => property.Name == "Name").Value);
            Assert.Contains(component.Properties, property => property.Name == "Health");
            Assert.True(history.Redo());
            Assert.Same(newBase, component.BaseInheritedComponent);

            component.Interfaces.Add(typeof(IUpdatable));
            component.Interfaces.Add(typeof(IDrawable));
            Func<CreateComponentTypeDialog, Task<bool>> removeCapabilities = dialog =>
            {
                var boxes = dialog.FindControl<StackPanel>("EntityOptions").Children.OfType<CheckBox>().ToArray();
                Assert.All(boxes, box => Assert.True(box.IsChecked));
                boxes[0].IsChecked = false;
                boxes[1].IsChecked = false;
                return Task.FromResult(true);
            };
            Func<string, Task<bool>> accept = _ => Task.FromResult(true);
            ((Task)execute.Invoke(command, new object[] { data, removeCapabilities, accept })).GetAwaiter().GetResult();
            Assert.False(component.HasInterface(typeof(IUpdatable)));
            Assert.False(component.HasInterface(typeof(IDrawable)));
            Func<CreateComponentTypeDialog, Task<bool>> checkReopened = dialog =>
            {
                var boxes = dialog.FindControl<StackPanel>("EntityOptions").Children.OfType<CheckBox>().ToArray();
                Assert.All(boxes, box => Assert.False(box.IsChecked));
                return Task.FromResult(false);
            };
            ((Task)execute.Invoke(command, new object[] { data, checkReopened, accept })).GetAwaiter().GetResult();
            Assert.True(history.Undo());
            Assert.True(component.HasInterface(typeof(IUpdatable)));
            Assert.True(component.HasInterface(typeof(IDrawable)));
            Func<CreateComponentTypeDialog, Task<bool>> checkUndo = dialog =>
            {
                var boxes = dialog.FindControl<StackPanel>("EntityOptions").Children.OfType<CheckBox>().ToArray();
                Assert.All(boxes, box => Assert.True(box.IsChecked));
                return Task.FromResult(false);
            };
            ((Task)execute.Invoke(command, new object[] { data, checkUndo, accept })).GetAwaiter().GetResult();
            Assert.True(history.Redo());
            ((Task)execute.Invoke(command, new object[] { data, checkReopened, accept })).GetAwaiter().GetResult();
        }
        finally
        {
            resourceProperty.SetValue(null, previous);
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
        public IEditorDialogModel Dialogs { get; set; }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (targetMethod.Name == nameof(IResourceModel.Get))
            {
                var type = targetMethod.GetGenericArguments()[0];
                if (type == typeof(IComponentProvider)) return Provider;
                if (type == typeof(IUINotifyModel)) return UINotify;
                if (type == typeof(IEditorDialogModel)) return Dialogs;
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
