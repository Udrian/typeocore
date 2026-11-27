using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using TypeD.Helpers;
using TypeD.Models.Data;
using TypeD.Models.Data.PropertyDefinitions;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Data.SaveContexts;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeD.View;
using TypeD.View.TreeNodes;
using TypeD.ViewModel;
using TypeDCore.Components;
using TypeDCore.Code.Entity;
using TypeDCore.Models.Data.Hooks;
using TypeDCore.Models.Data.SettingContexts;
using TypeDCore.Models.Data.PropertyDefinitions;
using TypeDCore.Models.Interfaces;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;
using TypeOEngine.Typedeaf.Core.Interfaces;

namespace TypeOCoreTest;

[Collection("EditorStaticResources")]
public class ComponentCreationTests
{
    [Fact]
    public async Task ComponentRename_LoadsSavedProjectAndPreparesSemanticChanges()
    {
        var directory = Path.Combine(Path.GetTempPath(), "TypeORenameProject-" + Guid.NewGuid().ToString("N"));
        var source = Path.Combine(directory, "Game");
        var feed = Path.Combine(directory, "empty-feed");
        Directory.CreateDirectory(source);
        Directory.CreateDirectory(feed);
        try
        {
            var project = MakeProject();
            typeof(Project).GetProperty(nameof(Project.Location)).SetValue(project, directory);
            File.WriteAllText(project.ProjectCSProjPath,
                "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>" +
                "<ItemGroup><Compile Include=\"..\\TypeO\\code\\Game\\Actor.typed.cs\" /></ItemGroup></Project>");
            var actor = Path.Combine(source, "Actor.cs");
            var consumer = Path.Combine(source, "Consumer.cs");
            var generated = Path.Combine(directory, "TypeO", "code", "Game", "Actor.typed.cs");
            Directory.CreateDirectory(Path.GetDirectoryName(generated));
            const string generatedText = "namespace Game; public partial class Actor { public int Health { get; set; } }";
            File.WriteAllText(generated, generatedText);
            File.WriteAllText(actor, "namespace Game; public partial class Actor { }");
            File.WriteAllText(consumer, "class Consumer { Game.Actor actor = new Game.Actor(); int Value() => actor.Health; }");
            var start = new ProcessStartInfo("dotnet")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            foreach (var argument in new[] { "restore", project.ProjectCSProjPath, "--source", feed, "--nologo" })
                start.ArgumentList.Add(argument);
            using var process = Process.Start(start);
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            Assert.True(process.ExitCode == 0, (await output) + (await error));

            var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.PropertyCodeRename", true);
            var method = type.GetMethod("CreateComponentAsync", BindingFlags.Static | BindingFlags.Public);
            var task = (Task)method.Invoke(null, new object[]
            {
                project, "Game.Actor", new[] { (OldName: "Health", NewName: "Speed") }, "Hero", "Game.Characters"
            });
            await task;
            var rename = task.GetType().GetProperty("Result").GetValue(task);
            type.GetMethod("Apply").Invoke(rename, null);
            Assert.Contains("class Hero", File.ReadAllText(actor));
            Assert.Equal(generatedText, File.ReadAllText(generated));
            Assert.Contains("Game.Characters.Hero", File.ReadAllText(consumer));
            Assert.Contains("actor.Speed", File.ReadAllText(consumer));
            type.GetMethod("Undo").Invoke(rename, null);
            Assert.Contains("class Actor", File.ReadAllText(actor));
            Assert.Contains("actor.Health", File.ReadAllText(consumer));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ComponentFileChanges_ApplyUndoAndRejectExternalEdits()
    {
        var directory = Path.Combine(Path.GetTempPath(), "TypeOComponentFiles-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var oldPath = Path.Combine(directory, "old.component");
            var newPath = Path.Combine(directory, "nested", "new.component");
            var before = System.Text.Encoding.UTF8.GetBytes("old");
            var after = System.Text.Encoding.UTF8.GetBytes("new");
            File.WriteAllBytes(oldPath, before);
            var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.ComponentFileChangeSet", true);
            var changes = new (string Path, byte[] Before, byte[] After)[]
            {
                (oldPath, before, null), (newPath, null, after)
            };
            var plan = Activator.CreateInstance(type, new object[] { directory, changes });
            void Invoke(string name) => type.GetMethod(name).Invoke(plan, null);
            Invoke("Apply");
            Assert.False(File.Exists(oldPath));
            Assert.Equal("new", File.ReadAllText(newPath));
            File.WriteAllText(newPath, "external");
            Assert.IsType<IOException>(Assert.Throws<TargetInvocationException>(() => Invoke("Undo")).InnerException);
            Assert.False(File.Exists(oldPath));
            File.WriteAllBytes(newPath, after);
            Invoke("Undo");
            Assert.Equal("old", File.ReadAllText(oldPath));
            Assert.False(File.Exists(newPath));
            Invoke("Apply");
            Assert.Equal("new", File.ReadAllText(newPath));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ComponentFileChanges_RollBackEarlierWritesWhenLaterWriteFails()
    {
        var directory = Path.Combine(Path.GetTempPath(), "TypeOComponentFiles-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var file = Path.Combine(directory, "original.component");
            var blocked = Path.Combine(directory, "not-a-directory");
            File.WriteAllText(file, "original");
            File.WriteAllText(blocked, "keep");
            var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.ComponentFileChangeSet", true);
            var changes = new (string Path, byte[] Before, byte[] After)[]
            {
                (file, System.Text.Encoding.UTF8.GetBytes("original"), System.Text.Encoding.UTF8.GetBytes("changed")),
                (Path.Combine(blocked, "new.component"), null, System.Text.Encoding.UTF8.GetBytes("new"))
            };
            var plan = Activator.CreateInstance(type, new object[] { directory, changes });
            Assert.IsAssignableFrom<IOException>(
                Assert.Throws<TargetInvocationException>(() => type.GetMethod("Apply").Invoke(plan, null)).InnerException);
            Assert.Equal("original", File.ReadAllText(file));
            Assert.Equal("keep", File.ReadAllText(blocked));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ComponentFileChanges_RejectDestinationCollisionWithoutChangingOtherFiles()
    {
        var directory = Path.Combine(Path.GetTempPath(), "TypeOComponentFiles-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source = Path.Combine(directory, "old.component");
            var destination = Path.Combine(directory, "new.component");
            var original = System.Text.Encoding.UTF8.GetBytes("original");
            File.WriteAllBytes(source, original);
            File.WriteAllText(destination, "existing");
            var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.ComponentFileChangeSet", true);
            var changes = new (string Path, byte[] Before, byte[] After)[]
            {
                (source, original, null), (destination, null, original)
            };
            var plan = Activator.CreateInstance(type, new object[] { directory, changes });
            Assert.IsType<IOException>(
                Assert.Throws<TargetInvocationException>(() => type.GetMethod("Apply").Invoke(plan, null)).InnerException);
            Assert.Equal("original", File.ReadAllText(source));
            Assert.Equal("existing", File.ReadAllText(destination));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ComponentFileChanges_RejectPathsOutsideProject()
    {
        var directory = Path.Combine(Path.GetTempPath(), "TypeOComponentFiles-" + Guid.NewGuid().ToString("N"));
        var outside = Path.Combine(directory + "-other", "new.component");
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.ComponentFileChangeSet", true);
        var changes = new (string Path, byte[] Before, byte[] After)[] { (outside, null, new byte[] { 1 }) };
        Assert.IsType<InvalidOperationException>(Assert.Throws<TargetInvocationException>(() =>
            Activator.CreateInstance(type, new object[] { directory, changes })).InnerException);
    }

    [Theory]
    [InlineData(CustomPropertyAccess.Public)]
    [InlineData(CustomPropertyAccess.Protected)]
    [InlineData(CustomPropertyAccess.Private)]
    public void ComponentPropertyAddition_UpdatesCopiesAndDerivedComponentsWithUndo(CustomPropertyAccess access)
    {
        var engine = CoreComponent.EntityComponent();
        var definition = new Component { Namespace = "Game", ClassName = "Actor", BaseInheritedComponent = engine };
        var instance = new Component { Namespace = "Game", ClassName = "Actor", BaseInheritedComponent = engine };
        var owner = new Component { Namespace = "Game", ClassName = "Scene" };
        owner.Children.Add(instance);
        var derived = new Component { Namespace = "Game", ClassName = "Enemy", BaseInheritedComponent = definition };
        var custom = new CustomProperty
        {
            Name = "Items", TypeName = typeof(List<int>).FullName,
            DefaultValue = new List<int> { 1 }, Access = access
        };
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.AddComponentPropertyPlan", true);
        var plan = Activator.CreateInstance(type, new object[]
        {
            new[] { definition, owner, derived }, definition.FullName, custom, typeof(List<int>)
        });
        void Invoke(string method) => type.GetMethod(method).Invoke(plan, null);
        Invoke("Apply");
        Assert.Single(definition.CustomProperties);
        Assert.Single(instance.CustomProperties);
        Assert.Empty(derived.CustomProperties);
        if (access == CustomPropertyAccess.Public)
        {
            var value = Assert.IsType<List<int>>(Assert.Single(definition.Properties).Value);
            value.Add(2);
            Assert.Equal(new[] { 1 }, Assert.IsType<List<int>>(Assert.Single(instance.Properties).Value));
            Assert.Equal(new[] { 1 }, Assert.IsType<List<int>>(Assert.Single(definition.OveriddenProperties).Value));
            Assert.Equal(new[] { 1 }, Assert.IsType<List<int>>(Assert.Single(definition.CustomProperties).DefaultValue));
            value.RemoveAt(1);
            Assert.Same(definition, Assert.Single(derived.Properties).FromComponent);
        }
        else
        {
            Assert.Empty(definition.Properties);
            Assert.Empty(instance.Properties);
            Assert.Empty(derived.Properties);
        }
        Invoke("Undo");
        Assert.Empty(definition.CustomProperties);
        Assert.Empty(instance.CustomProperties);
        Assert.Empty(derived.Properties);
        var collision = new CustomProperty { Name = "Items" };
        derived.CustomProperties.Add(collision);
        var failure = Assert.Throws<TargetInvocationException>(() => Invoke("Apply"));
        Assert.IsType<InvalidOperationException>(failure.InnerException);
        Assert.Empty(definition.CustomProperties);
        derived.CustomProperties.Remove(collision);
        Invoke("Apply");
        Assert.Single(definition.CustomProperties);
    }

    [Fact]
    public void ComponentPropertyAddition_RejectsConflictingDerivedProperty()
    {
        var definition = new Component { Namespace = "Game", ClassName = "Actor", BaseInheritedComponent = CoreComponent.EntityComponent() };
        var derived = new Component { Namespace = "Game", ClassName = "Enemy", BaseInheritedComponent = definition };
        derived.CustomProperties.Add(new CustomProperty { Name = "Health" });
        var custom = new CustomProperty { Name = "Health", TypeName = typeof(int).FullName, DefaultValue = 5 };
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.AddComponentPropertyPlan", true);
        var error = Assert.Throws<TargetInvocationException>(() => Activator.CreateInstance(type,
            new object[] { new[] { definition, derived }, definition.FullName, custom, typeof(int) }));
        Assert.IsType<InvalidOperationException>(error.InnerException);
        Assert.Empty(definition.CustomProperties);
        derived.CustomProperties.Clear();
        definition.BaseInheritedComponent.CustomProperties.Add(new CustomProperty { Name = "Health", Access = CustomPropertyAccess.Protected });
        error = Assert.Throws<TargetInvocationException>(() => Activator.CreateInstance(type,
            new object[] { new[] { definition, derived }, definition.FullName, custom, typeof(int) }));
        Assert.IsType<InvalidOperationException>(error.InnerException);
        Assert.Empty(definition.CustomProperties);
    }

    [Fact]
    public void ComponentPropertyAddition_RejectsUndoAfterValueChanges()
    {
        var definition = new Component
        {
            Namespace = "Game", ClassName = "Actor", BaseInheritedComponent = CoreComponent.EntityComponent()
        };
        var custom = new CustomProperty { Name = "Health", TypeName = typeof(int).FullName, DefaultValue = 5 };
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.AddComponentPropertyPlan", true);
        var plan = Activator.CreateInstance(type, new object[] { new[] { definition }, definition.FullName, custom, typeof(int) });
        type.GetMethod("Apply").Invoke(plan, null);
        definition.Properties.Single().Value = 10;

        var failure = Assert.Throws<TargetInvocationException>(() => type.GetMethod("Undo").Invoke(plan, null));
        Assert.IsType<InvalidOperationException>(failure.InnerException);
        Assert.Equal(10, definition.Properties.Single().Value);
        Assert.Single(definition.CustomProperties);

        definition.Properties.Single().Value = 5;
        type.GetMethod("Undo").Invoke(plan, null);
        type.GetMethod("Apply").Invoke(plan, null);
        Assert.Equal(5, definition.Properties.Single().Value);
    }

    [Fact]
    public void EditComponentDraft_AllowsEditingCustomProperties()
    {
        var root = CoreComponent.EntityComponent();
        var component = new Component { Namespace = "Game", ClassName = "Actor", BaseInheritedComponent = root };
        component.CustomProperties.Add(new CustomProperty { Name = "Existing", TypeName = typeof(int).FullName, DefaultValue = 5 });
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.ViewModel.Dialogs.Project.CreateComponentTypeViewModel", true);
        var model = Activator.CreateInstance(type, new object[]
        {
            null, MakeProject(), component.Namespace, root, new[] { component }, new Dictionary<string, string>()
        });
        type.GetMethod("InitializeEditing").Invoke(model, new object[] { component });
        var rows = (System.Collections.IList)type.GetProperty("PropertyRows").GetValue(model);
        type.GetProperty("SelectedProperty").SetValue(model, rows[0]);
        Assert.True((bool)type.GetProperty("CanEditDraftProperty").GetValue(model));

        var added = new CustomProperty { Name = "New", TypeName = typeof(int).FullName, DefaultValue = 7 };
        type.GetMethod("AddProperty").Invoke(model, new object[] { added, typeof(int), false });
        rows = (System.Collections.IList)type.GetProperty("PropertyRows").GetValue(model);
        var row = rows.Cast<object>().Single(item => ReferenceEquals(item.GetType().GetProperty("Property").GetValue(item), added));
        type.GetProperty("SelectedProperty").SetValue(model, row);
        Assert.True((bool)type.GetProperty("CanEditDraftProperty").GetValue(model));
    }

    [Fact]
    public async Task ComponentRenames_UseSemanticReferencesAndSupportPropertyNameSwaps()
    {
        using var workspace = new Microsoft.CodeAnalysis.AdhocWorkspace();
        var project = workspace.AddProject("Game", Microsoft.CodeAnalysis.LanguageNames.CSharp)
            .WithCompilationOptions(new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(
                Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary))
            .AddMetadataReference(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        var document = project.AddDocument("Actor.cs",
            "namespace Game; public class Actor { public Actor() {} public int Health { get; set; } public int Speed { get; set; } " +
            "public int Read(Other other) => Health * 10 + Speed + other.Health; public string Text = \"Health\"; } " +
            "public class Other { public int Health { get; set; } } public class Use { Actor Create() => new Actor(); }");
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.PropertyCodeRename", true);
        var method = type.GetMethod("RenameSymbolsAsync", BindingFlags.Static | BindingFlags.NonPublic);
        var task = (Task)method.Invoke(null, new object[]
        {
            document.Project, "Game.Actor", new[] { ("Health", "Speed"), ("Speed", "Health") }, "Enemy"
        });
        await task;
        var solution = (Microsoft.CodeAnalysis.Solution)task.GetType().GetProperty("Result").GetValue(task);
        var text = (await solution.GetDocument(document.Id).GetTextAsync()).ToString();
        Assert.Contains("class Enemy", text);
        Assert.Contains("public Enemy()", text);
        Assert.Contains("=> Speed * 10 + Health + other.Health", text);
        Assert.Contains("\"Health\"", text);
        Assert.Contains("Enemy Create() => new Enemy()", text);
    }

    [Fact]
    public async Task ComponentNamespaceMove_UpdatesDeclarationsAndSemanticReferences()
    {
        using var workspace = new Microsoft.CodeAnalysis.AdhocWorkspace();
        var project = workspace.AddProject("Game", Microsoft.CodeAnalysis.LanguageNames.CSharp)
            .WithCompilationOptions(new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(
                Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary))
            .AddMetadataReference(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        var declaration = project.AddDocument("Actor.cs",
            "namespace Game.Entities; public partial class Actor { public Actor Copy() => new Actor(); public string Text = \"Actor\"; }");
        var partial = declaration.Project.AddDocument("Actor.extra.cs",
            "namespace Game.Entities { public partial class Actor { } }");
        var use = partial.Project.AddDocument("Use.cs",
            "namespace Game.Entities { public class Use { Actor a = new Game.Entities.Actor(); string Text = \"Game.Entities.Actor\"; } }");
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.PropertyCodeRename", true);
        var method = type.GetMethod("MoveComponentNamespaceAsync", BindingFlags.Static | BindingFlags.NonPublic);
        var task = (Task)method.Invoke(null, new object[] { use.Project, "Game.Entities.Actor", "Game.Characters" });
        await task;
        var solution = (Microsoft.CodeAnalysis.Solution)task.GetType().GetProperty("Result").GetValue(task);
        var actorText = (await solution.GetDocument(declaration.Id).GetTextAsync()).ToString();
        var partialText = (await solution.GetDocument(partial.Id).GetTextAsync()).ToString();
        var useText = (await solution.GetDocument(use.Id).GetTextAsync()).ToString();
        Assert.Contains("namespace Game.Characters;", actorText);
        Assert.Contains("global::Game.Characters.Actor Copy()", actorText);
        Assert.Contains("namespace Game.Characters", partialText);
        Assert.Contains("global::Game.Characters.Actor a = new global::Game.Characters.Actor()", useText);
        Assert.Contains("\"Game.Entities.Actor\"", useText);
        var compilation = await solution.GetProject(use.Project.Id).GetCompilationAsync();
        Assert.DoesNotContain(compilation.GetDiagnostics(),
            diagnostic => diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
    }

    [Fact]
    public async Task ComponentNamespaceMove_RejectsMovingOtherTypes()
    {
        using var workspace = new Microsoft.CodeAnalysis.AdhocWorkspace();
        var project = workspace.AddProject("Game", Microsoft.CodeAnalysis.LanguageNames.CSharp)
            .AddMetadataReference(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddDocument("Actor.cs", "namespace Game { public class Actor {} public class Other {} }").Project;
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.PropertyCodeRename", true);
        var method = type.GetMethod("MoveComponentNamespaceAsync", BindingFlags.Static | BindingFlags.NonPublic);
        var task = (Task)method.Invoke(null, new object[] { project, "Game.Actor", "Game.Characters" });
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);
        Assert.Contains("only type", error.Message);
        Assert.Contains("class Other", (await project.Documents.Single().GetTextAsync()).ToString());
    }

    [Fact]
    public async Task ComponentNamespaceMove_UpdatesReferencesInDependentProjects()
    {
        using var workspace = new Microsoft.CodeAnalysis.AdhocWorkspace();
        var game = workspace.AddProject("Game", Microsoft.CodeAnalysis.LanguageNames.CSharp)
            .WithCompilationOptions(new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(
                Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary))
            .AddMetadataReference(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddDocument("Actor.cs", "namespace Game { public class Actor {} }").Project;
        var otherId = Microsoft.CodeAnalysis.ProjectId.CreateNewId();
        var solution = game.Solution.AddProject(otherId, "Other", "Other", Microsoft.CodeAnalysis.LanguageNames.CSharp)
            .WithProjectCompilationOptions(otherId, new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(
                Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary))
            .AddMetadataReference(otherId, Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddProjectReference(otherId, new Microsoft.CodeAnalysis.ProjectReference(game.Id));
        var documentId = Microsoft.CodeAnalysis.DocumentId.CreateNewId(otherId);
        solution = solution.AddDocument(documentId, "Use.cs", "class Use { Game.Actor actor = new Game.Actor(); }");
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.PropertyCodeRename", true);
        var method = type.GetMethod("MoveComponentNamespaceAsync", BindingFlags.Static | BindingFlags.NonPublic);
        var task = (Task)method.Invoke(null, new object[] { solution.GetProject(game.Id), "Game.Actor", "Game.Entities" });
        await task;
        var result = (Microsoft.CodeAnalysis.Solution)task.GetType().GetProperty("Result").GetValue(task);
        var text = (await result.GetDocument(documentId).GetTextAsync()).ToString();
        Assert.Contains("global::Game.Entities.Actor actor = new global::Game.Entities.Actor()", text);
        var compilation = await result.GetProject(otherId).GetCompilationAsync();
        Assert.DoesNotContain(compilation.GetDiagnostics(),
            diagnostic => diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
    }

    [Fact]
    public async Task ComponentNamespaceMove_CanFollowClassAndPropertyRenames()
    {
        using var workspace = new Microsoft.CodeAnalysis.AdhocWorkspace();
        var document = workspace.AddProject("Game", Microsoft.CodeAnalysis.LanguageNames.CSharp)
            .WithCompilationOptions(new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(
                Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary))
            .AddMetadataReference(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddDocument("Actor.cs", "namespace Game; public class Actor { public int Health { get; set; } public Actor Copy() => new Actor { Health = Health }; }");
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.PropertyCodeRename", true);
        var rename = type.GetMethod("RenameSymbolsAsync", BindingFlags.Static | BindingFlags.NonPublic);
        var renameTask = (Task)rename.Invoke(null, new object[]
        {
            document.Project, "Game.Actor", new[] { ("Health", "HitPoints") }, "Enemy"
        });
        await renameTask;
        var renamed = (Microsoft.CodeAnalysis.Solution)renameTask.GetType().GetProperty("Result").GetValue(renameTask);
        var move = type.GetMethod("MoveComponentNamespaceAsync", BindingFlags.Static | BindingFlags.NonPublic);
        var moveTask = (Task)move.Invoke(null, new object[] { renamed.GetProject(document.Project.Id), "Game.Enemy", "Game.Entities" });
        await moveTask;
        var moved = (Microsoft.CodeAnalysis.Solution)moveTask.GetType().GetProperty("Result").GetValue(moveTask);
        var text = (await moved.GetDocument(document.Id).GetTextAsync()).ToString();
        Assert.Contains("namespace Game.Entities;", text);
        Assert.Contains("class Enemy", text);
        Assert.Contains("HitPoints = HitPoints", text);
        Assert.Contains("global::Game.Entities.Enemy Copy()", text);
        var compilation = await moved.GetProject(document.Project.Id).GetCompilationAsync();
        Assert.DoesNotContain(compilation.GetDiagnostics(),
            diagnostic => diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
    }

    [Fact]
    public void IdentityPlan_UpdatesCopiesDerivedReferencesAndStartingSceneWithUndo()
    {
        var project = MakeProject();
        project.StartScene = "Game.Level";
        var sceneRoot = CoreComponent.SceneComponent();
        var definition = new Component { Namespace = "Game", ClassName = "Level", BaseInheritedComponent = sceneRoot };
        var instance = new Component { Namespace = "Game", ClassName = "Level", BaseInheritedComponent = sceneRoot };
        var inheritedCopy = new Component { Namespace = "Game", ClassName = "Level", BaseInheritedComponent = sceneRoot };
        var derived = new Component { Namespace = "Game", ClassName = "BossLevel", BaseInheritedComponent = inheritedCopy };
        var game = new Component { Namespace = "Game", ClassName = "Game", BaseInheritedComponent = CoreComponent.GameComponent() };
        game.Children.Add(instance);
        var start = new Property { Name = "StartingScene", Value = "Game.Level",
            DeclaringTypeName = typeof(TypeOEngine.Typedeaf.Core.Game).FullName };
        var baseline = new Property(start);
        var text = new Property { Name = "Label", Value = "Game.Level" };
        game.Properties.AddRange(new[] { start, text });
        game.OveriddenProperties.Add(baseline);
        var property = new Property { Name = "Health", DeclaringTypeName = "Game.Level" };
        derived.Properties.Add(property);
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.Commands.ComponentIdentityPlan", true);
        var plan = Activator.CreateInstance(type, new object[]
        {
            project, new[] { definition, game, derived }, "Game.Level", "Game.Levels", "Arena"
        });
        void Invoke(string method) => type.GetMethod(method).Invoke(plan, null);
        Assert.Equal(3, ((IReadOnlyList<Component>)type.GetProperty("AffectedRoots").GetValue(plan)).Count);

        Invoke("Apply");
        Assert.Equal("Game.Levels.Arena", definition.FullName);
        Assert.Equal("Game.Levels.Arena", instance.FullName);
        Assert.Equal("Game.Levels.Arena", derived.BaseInheritedComponent.FullName);
        Assert.Equal("Game.Levels.Arena", property.DeclaringTypeName);
        Assert.Equal("Game.Levels.Arena", start.Value);
        Assert.Equal("Game.Levels.Arena", baseline.Value);
        Assert.Equal("Game.Levels.Arena", project.StartScene);
        Assert.Equal("Game.Level", text.Value);
        Invoke("Undo");
        Assert.Equal("Game.Level", definition.FullName);
        Assert.Equal("Game.Level", instance.FullName);
        Assert.Equal("Game.Level", property.DeclaringTypeName);
        Assert.Equal("Game.Level", start.Value);
        Assert.Equal("Game.Level", project.StartScene);
        start.Value = "Game.Other";
        var failure = Assert.Throws<TargetInvocationException>(() => Invoke("Apply"));
        Assert.IsType<InvalidOperationException>(failure.InnerException);
        Assert.Equal("Game.Level", definition.FullName);
    }

    [Fact]
    public void EditingDraft_TracksOriginalPropertiesAndRejectsInheritanceCycles()
    {
        var root = CoreComponent.EntityComponent();
        var component = new Component
        {
            Namespace = "Game.Entities", ClassName = "Actor", BaseInheritedComponent = root, Icon = "actor.png"
        };
        component.Interfaces.Add(typeof(IUpdatable));
        var original = new CustomProperty { Name = "Health", TypeName = typeof(int).FullName, DefaultValue = 5 };
        component.CustomProperties.Add(original);
        var derived = new Component { Namespace = "Game", ClassName = "Enemy", BaseInheritedComponent = component };
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.ViewModel.Dialogs.Project.CreateComponentTypeViewModel", true);
        var model = Activator.CreateInstance(type, new object[]
        {
            null, MakeProject(), component.Namespace, root, new[] { component, derived }, new Dictionary<string, string>()
        });
        object Get(string name) => type.GetProperty(name).GetValue(model);
        type.GetMethod("InitializeEditing").Invoke(model, new object[] { component });

        Assert.Equal("Actor", Get("ComponentName"));
        Assert.Equal("Game.Entities", Get("ComponentNamespace"));
        Assert.Equal("actor.png", Get("IconName"));
        Assert.Equal(true, Get("ComponentUpdatable"));
        Assert.Equal(false, Get("ComponentDrawable"));
        Assert.DoesNotContain(derived.FullName, (IReadOnlyList<string>)Get("BaseTypeSuggestions"));
        var draft = Assert.Single((IReadOnlyList<CustomProperty>)Get("CustomProperties"));
        Assert.NotSame(original, draft);
        Assert.Same(original, type.GetMethod("OriginalProperty").Invoke(model, new object[] { draft }));
        var replacement = new CustomProperty(draft) { Name = "HitPoints" };
        type.GetMethod("ReplaceProperty").Invoke(model, new object[] { draft, replacement, typeof(int) });
        Assert.Same(original, type.GetMethod("OriginalProperty").Invoke(model, new object[] { replacement }));
        Assert.Equal("Health", original.Name);
        type.GetMethod("OnParentComponentSet").Invoke(model, new object[] { derived });
        Assert.False((bool)type.GetMethod("Validate").Invoke(model, null));
        Assert.Contains("cannot inherit", (string)Get("ValidationError"));
    }

    [Fact]
    public void DraftProperties_ShowInheritedAndRejectConflictsAfterChangingBase()
    {
        var root = CoreComponent.EntityComponent();
        var parent = new Component { Namespace = "Game", ClassName = "Actor", BaseInheritedComponent = root };
        var visible = new CustomProperty { Name = "Health", Access = CustomPropertyAccess.Public };
        var protectedProperty = new CustomProperty { Name = "Speed", Access = CustomPropertyAccess.Protected };
        var hidden = new CustomProperty { Name = "Secret", Access = CustomPropertyAccess.Private };
        parent.CustomProperties.AddRange(new[] { visible, protectedProperty, hidden });
        var alternate = new Component { Namespace = "Game", ClassName = "Other", BaseInheritedComponent = root };
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.ViewModel.Dialogs.Project.CreateComponentTypeViewModel", true);
        var view = Activator.CreateInstance(type, new object[] { null, MakeProject(), null, parent, new[] { parent, alternate }, new Dictionary<string, string>() });
        object Get(string name) => type.GetProperty(name).GetValue(view);
        void Set(string name, object value) => type.GetProperty(name).SetValue(view, value);
        var rows = ((System.Collections.IEnumerable)Get("PropertyRows")).Cast<object>().ToList();
        Assert.Equal(2, rows.Count);
        Assert.All(rows, row => Assert.True((bool)row.GetType().GetProperty("IsInherited").GetValue(row)));
        Assert.Equal(" (inherited from Game.Actor)", rows[0].GetType().GetProperty("InheritedFrom").GetValue(rows[0]));
        Assert.DoesNotContain("inherited", (string)rows[0].GetType().GetProperty("Details").GetValue(rows[0]));
        Set("SelectedProperty", rows[0]);
        Assert.Equal(false, Get("CanEditProperty"));
        var taken = (HashSet<string>)type.GetMethod("TakenPropertyNames").Invoke(view, new object[] { null, Array.Empty<string>() });
        Assert.Contains("Secret", taken);

        Set("ComponentName", "Enemy");
        var draft = new CustomProperty { Name = "Damage", TypeName = typeof(int).FullName, DefaultValue = 7 };
        type.GetMethod("AddProperty").Invoke(view, new object[] { draft, typeof(int), true });
        var draftRow = ((System.Collections.IEnumerable)Get("PropertyRows")).Cast<object>().Last();
        Assert.Equal("", draftRow.GetType().GetProperty("InheritedFrom").GetValue(draftRow));
        var edited = new CustomProperty { Name = "Power", TypeName = typeof(int).FullName, DefaultValue = 8 };
        type.GetMethod("ReplaceProperty").Invoke(view, new object[] { draft, edited, typeof(int) });
        Assert.Equal("Power", Assert.Single((IReadOnlyList<CustomProperty>)Get("CustomProperties")).Name);
        Assert.Empty(alternate.CustomProperties);
        type.GetMethod("OnParentComponentSet").Invoke(view, new object[] { alternate });
        Assert.Single(((System.Collections.IEnumerable)Get("PropertyRows")).Cast<object>());
        Assert.True((bool)type.GetMethod("Validate").Invoke(view, null));
        Assert.False((bool)type.GetMethod("ValidatePropertyNames").Invoke(view, new object[] { new[] { "Power" } }));
        Assert.Contains("Power", (string)Get("ValidationError"));
        Set("ComponentName", "Power");
        Assert.False((bool)type.GetMethod("Validate").Invoke(view, null));
        Set("ComponentName", "Enemy");
        parent.CustomProperties.Add(new CustomProperty { Name = "Power" });
        type.GetMethod("OnParentComponentSet").Invoke(view, new object[] { parent });
        Assert.False((bool)type.GetMethod("Validate").Invoke(view, null));
        Assert.Contains("Power", (string)Get("ValidationError"));
        type.GetMethod("RemoveProperty").Invoke(view, new object[] { edited });
        Assert.Empty((IReadOnlyList<CustomProperty>)Get("CustomProperties"));
        Assert.Null(Get("PropertyPreference"));
        Assert.True((bool)type.GetMethod("Validate").Invoke(view, null));
        Assert.DoesNotContain(edited, parent.CustomProperties);
    }

    [Fact]
    public async Task Creation_PassesTypedDraftsToHookAndRecreatesThemOnRedo()
    {
        var root = CoreComponent.EntityComponent();
        var vector = new Vec2(3.5, -2);
        var property = new CustomProperty { Name = "Offset", TypeName = typeof(Vec2).FullName, DefaultValue = vector, Group = "Stats" };
        var secret = new CustomProperty { Name = "Secret", TypeName = typeof(int).FullName, DefaultValue = 42, Access = CustomPropertyAccess.Private };
        var options = new ComponentCreationOptions
        {
            Name = "Actor", Namespace = "Game", ParentComponent = root, BaseType = root.FullName,
            CustomProperties = new List<CustomProperty> { property, secret },
            PropertyPreference = (property, typeof(Vec2), true)
        };
        var dialog = DispatchProxy.Create<IEditorDialogModel, CallbackProxy>();
        ((CallbackProxy)dialog).Callback = (_, _) => Task.FromResult(options);
        var properties = DispatchProxy.Create<IPropertyModel, CallbackProxy>();
        ((CallbackProxy)properties).Callback = (_, _) => new IPropertyDefinition[] { new Int32PropertyDefinition(), new Vec2PropertyDefinition() };
        var created = new List<Component>();
        var model = DispatchProxy.Create<ITypeDCoreProjectModel, CallbackProxy>();
        ((CallbackProxy)model).Callback = (_, args) =>
        {
            var component = new Component { Namespace = "Game", ClassName = "Actor", CustomProperties = (List<CustomProperty>)args[^1] };
            created.Add(component);
            return component;
        };
        var provider = DispatchProxy.Create<IComponentProvider, CallbackProxy>();
        ((CallbackProxy)provider).Callback = (_, _) => null;
        var hooks = (IHookModel)Activator.CreateInstance(typeof(Component).Assembly.GetType("TypeD.Models.HookModel", true));
        hooks.AddHook<CreateComponentHook>(hook =>
        {
            Assert.Equal(2, hook.CreateComponentDTO.CustomProperties.Count);
            Assert.Equal(vector, Assert.IsType<Vec2>(hook.CreateComponentDTO.CustomProperties[0].DefaultValue));
        });
        var history = (ICommandHistoryModel)Activator.CreateInstance(typeof(Component).Assembly.GetType("TypeD.Models.CommandHistoryModel", true));
        var componentSettings = new ComponentCreationSettingContext();
        var propertySettings = new PropertyCreationSettingContext();
        var settings = DispatchProxy.Create<ISettingModel, CallbackProxy>();
        ((CallbackProxy)settings).Callback = (method, _) =>
        {
            if (method.Name == nameof(ISettingModel.SetContext)) return null;
            return method.GetGenericArguments()[0] == typeof(ComponentCreationSettingContext) ? componentSettings : propertySettings;
        };
        var resources = DispatchProxy.Create<IResourceModel, CallbackProxy>();
        ((CallbackProxy)resources).Callback = (method, _) =>
        {
            var service = method.GetGenericArguments()[0];
            if (service == typeof(IEditorDialogModel)) return dialog;
            if (service == typeof(IPropertyModel)) return properties;
            if (service == typeof(ITypeDCoreProjectModel)) return model;
            if (service == typeof(IComponentProvider)) return provider;
            if (service == typeof(IHookModel)) return hooks;
            if (service == typeof(ICommandHistoryModel)) return history;
            if (service == typeof(ISettingModel)) return settings;
            throw new InvalidOperationException(service.Name);
        };
        var assembly = typeof(CoreComponent).Assembly;
        var data = Activator.CreateInstance(assembly.GetType("TypeDCore.Commands.Data.CreateComponentCommandData", true), new object[] { MakeProject(), null });
        await (Task)assembly.GetType("TypeDCore.Commands.CreateComponentTypeCommand", true)
            .GetMethod("ExecuteAsync", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { data, null, resources });

        Assert.Equal(vector, Assert.IsType<Vec2>(created[0].CustomProperties[0].DefaultValue));
        Assert.Equal("Stats", propertySettings.Group.Value);
        Assert.Equal(vector, System.Text.Json.JsonSerializer.Deserialize<Vec2>(propertySettings.DefaultValueJson.Value, JSON.CreateOptions()));
        created[0].CustomProperties[0].DefaultValue = new Vec2(9, 9);
        Assert.True(history.Undo());
        Assert.True(history.Redo());
        Assert.Equal(vector, Assert.IsType<Vec2>(created[1].CustomProperties[0].DefaultValue));
        Assert.Equal(CustomPropertyAccess.Private, created[1].CustomProperties[1].Access);
    }
    [Fact]
    public void BaseAutocomplete_FiltersRootAndRequiresAnUnambiguousChoice()
    {
        var root = CoreComponent.EntityComponent();
        var scene = CoreComponent.SceneComponent();
        var entity2d = new Component { Namespace = "Engine", ClassName = "Entity2d", BaseInheritedComponent = root };
        entity2d.Interfaces.Add(typeof(IUpdatable));
        var other = new Component { Namespace = "Game", ClassName = "Entity2d", BaseInheritedComponent = root };
        var sceneBase = new Component { Namespace = "Game", ClassName = "Level", BaseInheritedComponent = scene };
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.ViewModel.Dialogs.Project.CreateComponentTypeViewModel", true);
        var viewModel = Activator.CreateInstance(type, new object[]
        {
            null, MakeProject(), null, root, new[] { entity2d, other, sceneBase }, new Dictionary<string, string>()
        });
        object Get(string name) => type.GetProperty(name).GetValue(viewModel);
        void Set(string name, object value) => type.GetProperty(name).SetValue(viewModel, value);
        var suggestions = (IReadOnlyList<string>)Get("BaseTypeSuggestions");
        Assert.Contains(entity2d.FullName, suggestions);
        Assert.Contains(other.FullName, suggestions);
        Assert.DoesNotContain(sceneBase.FullName, suggestions);
        Set("ComponentName", "Player");
        Set("BaseTypeText", "Entity2d");
        type.GetMethod("CommitBaseTypeText").Invoke(viewModel, null);
        Assert.False((bool)type.GetMethod("Validate").Invoke(viewModel, null));
        Assert.Contains("Select an existing base type", (string)Get("ValidationError"));
        Set("BaseTypeText", entity2d.FullName);
        type.GetMethod("CommitBaseTypeText").Invoke(viewModel, null);
        Assert.Same(entity2d, Get("ParentComponent"));
        Assert.Equal("Entity2d", Get("BaseTypeText"));
        type.GetMethod("CommitBaseTypeText").Invoke(viewModel, null);
        Assert.Same(entity2d, Get("ParentComponent"));
        Assert.Equal(true, Get("ComponentUpdatable"));
        Assert.Equal(false, Get("ComponentUpdatableEnabled"));
        Assert.True((bool)type.GetMethod("Validate").Invoke(viewModel, null));

        var roots = ((IEnumerable<Component>)Get("RootTypes")).ToList();
        Set("SelectedRoot", roots[1]);
        suggestions = (IReadOnlyList<string>)Get("BaseTypeSuggestions");
        Assert.Contains(sceneBase.FullName, suggestions);
        Assert.DoesNotContain(entity2d.FullName, suggestions);
        Set("BaseTypeText", "level");
        type.GetMethod("CommitBaseTypeText").Invoke(viewModel, null);
        Assert.Same(sceneBase, Get("ParentComponent"));
        Assert.Equal("Level", Get("BaseTypeText"));
        Set("SelectedRoot", roots[0]);
        Assert.Same(entity2d, Get("ParentComponent"));
        Set("BaseTypeText", "Other");
        Assert.False((bool)type.GetMethod("Validate").Invoke(viewModel, null));
        Set("BaseTypeText", "Entity2d");
        type.GetMethod("CommitBaseTypeText").Invoke(viewModel, null);
        Assert.False((bool)type.GetMethod("Validate").Invoke(viewModel, null));
    }

    [Fact]
    public void RememberedBases_AreRestoredPerRootAndExplicitBaseTakesPrecedence()
    {
        var entity = CoreComponent.EntityComponent();
        var scene = CoreComponent.SceneComponent();
        var entity2d = new Component { Namespace = "Engine", ClassName = "Entity2d", BaseInheritedComponent = entity };
        var level = new Component { Namespace = "Game", ClassName = "Level", BaseInheritedComponent = scene };
        var explicitBase = new Component { Namespace = "Game", ClassName = "Actor", BaseInheritedComponent = entity };
        var moduleTypes = new[] { entity2d, level, explicitBase };
        var remembered = new Dictionary<string, string>
        {
            [entity.FullName] = entity2d.FullName,
            [scene.FullName] = level.FullName
        };
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.ViewModel.Dialogs.Project.CreateComponentTypeViewModel", true);
        var viewModel = Activator.CreateInstance(type, new object[] { null, MakeProject(), null, entity, moduleTypes, remembered });
        Assert.Same(entity2d, type.GetProperty("ParentComponent").GetValue(viewModel));
        var roots = ((IEnumerable<Component>)type.GetProperty("RootTypes").GetValue(viewModel)).ToList();
        type.GetProperty("SelectedRoot").SetValue(viewModel, roots[1]);
        Assert.Same(level, type.GetProperty("ParentComponent").GetValue(viewModel));
        var explicitView = Activator.CreateInstance(type, new object[] { null, MakeProject(), null, explicitBase, moduleTypes, remembered });
        Assert.Same(explicitBase, type.GetProperty("ParentComponent").GetValue(explicitView));

        remembered[entity.FullName] = "Unavailable.Entity2d";
        var staleView = Activator.CreateInstance(type, new object[] { null, MakeProject(), null, entity, moduleTypes, remembered });
        Assert.Equal(entity.FullName, ((Component)type.GetProperty("ParentComponent").GetValue(staleView)).FullName);
    }

    [Fact]
    public void RememberedFields_AreOptInAndContextTakesPrecedence()
    {
        var entity = CoreComponent.EntityComponent();
        var scene = CoreComponent.SceneComponent();
        var level = new Component { Namespace = "Game", ClassName = "Level", BaseInheritedComponent = scene };
        var settings = new ComponentCreationSettingContext();
        settings.RootType.Value = scene.FullName;
        settings.LastBaseTypes.Value[scene.FullName] = level.FullName;
        settings.Namespace.Value = "Game.Custom";
        settings.Icon.Value = "custom.png";
        settings.Updatable.Value = false;
        settings.Drawable.Value = false;
        settings.RememberFields.Value = false;
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.ViewModel.Dialogs.Project.CreateComponentTypeViewModel", true);
        object Create(string componentNamespace, Component initial) =>
            Activator.CreateInstance(type, new object[] { null, MakeProject(), componentNamespace, initial, new[] { level }, settings });
        object Get(object view, string name) => type.GetProperty(name).GetValue(view);

        var disabled = Create(null, null);
        Assert.Equal(entity.FullName, Get(disabled, "ComponentBaseType"));
        Assert.Equal("Game.Entities", Get(disabled, "ComponentNamespace"));
        Assert.Null(Get(disabled, "IconName"));
        Assert.Equal(true, Get(disabled, "ComponentUpdatable"));

        settings.RememberFields.Value = true;
        var restored = Create(null, null);
        Assert.Equal(scene.FullName, Get(restored, "ComponentBaseType"));
        Assert.Same(level, Get(restored, "ParentComponent"));
        Assert.Equal("Game.Custom", Get(restored, "ComponentNamespace"));
        Assert.Equal("custom.png", Get(restored, "IconName"));
        Assert.Equal(false, Get(restored, "ComponentUpdatable"));
        Assert.Equal(false, Get(restored, "ComponentDrawable"));
        Assert.True(string.IsNullOrEmpty((string)Get(restored, "ComponentName")));
        var explicitBase = new Component { Namespace = "Game", ClassName = "Actor", BaseInheritedComponent = entity };
        var contextual = Create("Game.Actors", explicitBase);
        Assert.Same(explicitBase, Get(contextual, "ParentComponent"));
        Assert.Equal(entity.FullName, Get(contextual, "ComponentBaseType"));
        Assert.Equal("Game.Actors", Get(contextual, "ComponentNamespace"));
    }

    [Fact]
    public async Task RememberedBases_AreSavedOnExitAndReloaded()
    {
        var project = MakeProject();
        typeof(Project).GetProperty(nameof(Project.ProjectName)).SetValue(project, "CreationSettingsTest-" + Guid.NewGuid().ToString("N"));
        var settingsType = typeof(Component).Assembly.GetType("TypeD.Models.SettingModel", true);
        var settings = (ISettingModel)Activator.CreateInstance(settingsType);
        var hooks = (IHookModel)Activator.CreateInstance(typeof(Component).Assembly.GetType("TypeD.Models.HookModel", true));
        var save = new SettingSaveContext();
        Task saveTask = null;
        var saves = DispatchProxy.Create<ISaveModel, CallbackProxy>();
        ((CallbackProxy)saves).Callback = (method, args) =>
        {
            if (method.Name == nameof(ISaveModel.GetSaveContext)) return save;
            if (method.Name == nameof(ISaveModel.SaveNow))
            {
                saveTask = save.SaveAction((Project)args[0]);
                return null;
            }
            throw new InvalidOperationException(method.Name);
        };
        var resources = DispatchProxy.Create<IResourceModel, CallbackProxy>();
        ((CallbackProxy)resources).Callback = (method, args) =>
        {
            var service = method.GetGenericArguments()[0];
            if (service == typeof(ISaveModel)) return saves;
            if (service == typeof(IHookModel)) return hooks;
            if (service == settingsType) return settings;
            throw new InvalidOperationException(service.Name);
        };
        save.Init(resources);
        settings.Init(resources);
        var folder = settings.GetPath(project, SettingLevel.User);
        var file = Path.Combine(folder, settings.GetName<ComponentCreationSettingContext>() + ".json");
        var propertyFile = Path.Combine(folder, settings.GetName<PropertyCreationSettingContext>() + ".json");
        Assert.False(Directory.Exists(folder));
        try
        {
            settings.InitContext<ComponentCreationSettingContext>(project);
            settings.InitContext<PropertyCreationSettingContext>(project);
            var context = settings.GetContext<ComponentCreationSettingContext>(SettingLevel.User);
            context.LastBaseTypes.Value["Entity"] = "Engine.Entity2d";
            context.RememberFields.Value = false;
            context.Namespace.Value = "Game.Entities";
            settings.SetContext(context);
            var propertySettings = settings.GetContext<PropertyCreationSettingContext>(SettingLevel.User);
            propertySettings.RememberFields.Value = false;
            propertySettings.TypeName.Value = typeof(int).FullName;
            propertySettings.DefaultValueJson.Value = "42";
            propertySettings.Group.Value = "Stats";
            settings.SetContext(propertySettings);
            hooks.Shoot(new ExitHook { Project = project });
            Assert.NotNull(saveTask);
            await saveTask;
            Assert.True(File.Exists(file));
            Assert.True(File.Exists(propertyFile));
            settings.InitContext<ComponentCreationSettingContext>(project);
            settings.InitContext<PropertyCreationSettingContext>(project);
            var reloaded = settings.GetContext<ComponentCreationSettingContext>();
            Assert.Equal("Engine.Entity2d", reloaded.LastBaseTypes.Value["Entity"]);
            Assert.False(reloaded.RememberFields.Value);
            Assert.Equal("Game.Entities", reloaded.Namespace.Value);
            var reloadedProperty = settings.GetContext<PropertyCreationSettingContext>();
            Assert.False(reloadedProperty.RememberFields.Value);
            Assert.Equal(typeof(int).FullName, reloadedProperty.TypeName.Value);
            Assert.Equal("42", reloadedProperty.DefaultValueJson.Value);
            Assert.Equal("Stats", reloadedProperty.Group.Value);
        }
        finally
        {
            if (File.Exists(file)) File.Delete(file);
            if (File.Exists(propertyFile)) File.Delete(propertyFile);
            if (Directory.Exists(folder)) Directory.Delete(folder);
        }
    }

    [Fact]
    public void RootSwitching_ResetsParentAndPreservesFieldsAndEntityChoices()
    {
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.ViewModel.Dialogs.Project.CreateComponentTypeViewModel", true);
        var project = MakeProject();
        var viewModel = Activator.CreateInstance(type, new object[] { null, project, null, CoreComponent.EntityComponent() });
        object Get(string name) => type.GetProperty(name).GetValue(viewModel);
        void Set(string name, object value) => type.GetProperty(name).SetValue(viewModel, value);
        var roots = ((IEnumerable<Component>)Get("RootTypes")).ToList();
        Assert.Equal(new[] { "Entity", "Scene", "Drawable" }, roots.Select(root => root.ClassName));
        Assert.Equal("Game.Entities", Get("ComponentNamespace"));
        Assert.Equal(true, Get("ComponentUpdatable"));
        Assert.Equal(true, Get("ComponentDrawable"));
        Set("ComponentUpdatable", false);
        Set("ComponentDrawable", false);
        Set("ComponentName", "Actor");
        var parent1 = new Component { ClassName = "First", Namespace = "Game", BaseInheritedComponent = roots[0] };
        var parent2 = new Component { ClassName = "Second", Namespace = "Game", BaseInheritedComponent = roots[0] };
        parent1.Interfaces.Add(typeof(IUpdatable));
        parent1.Interfaces.Add(typeof(IDrawable));
        parent2.Interfaces.Add(typeof(IUpdatable));
        parent2.Interfaces.Add(typeof(IDrawable));
        type.GetMethod("OnParentComponentSet").Invoke(viewModel, new object[] { parent1 });
        type.GetMethod("OnParentComponentSet").Invoke(viewModel, new object[] { parent2 });
        Assert.Equal(true, Get("ComponentUpdatable"));
        Assert.Equal(false, Get("ComponentUpdatableEnabled"));
        Set("SelectedRoot", roots[1]);
        Assert.Same(roots[1], Get("ParentComponent"));
        Assert.Equal(roots[1].FullName, Get("ParentComponentFullName"));
        Assert.Equal("Game.Scenes", Get("ComponentNamespace"));
        Assert.Equal("Actor", Get("ComponentName"));
        Assert.Equal(false, Get("ComponentUpdatable"));
        Assert.Equal(false, Get("ComponentDrawable"));
        Assert.Equal(true, Get("ComponentUpdatableEnabled"));
        Set("ComponentNamespace", "Custom.Folder");
        Set("SelectedRoot", roots[2]);
        Set("SelectedRoot", roots[0]);
        Assert.Equal("Custom.Folder", Get("ComponentNamespace"));
        Assert.Equal(true, Get("IsEntity"));
        Assert.Same(parent2, Get("ParentComponent"));
        Assert.Equal("Inherited from Second.", Get("UpdatableDescription"));
        Assert.Equal(true, Get("ComponentUpdatable"));
        type.GetMethod("OnParentComponentSet").Invoke(viewModel, new object[] { roots[0] });
        Assert.Equal(false, Get("ComponentUpdatable"));
        Assert.Equal(false, Get("ComponentDrawable"));

        var contextual = Activator.CreateInstance(type, new object[] { null, project, "Scenes", roots[0] });
        type.GetProperty("SelectedRoot").SetValue(contextual, roots[2]);
        Assert.Equal("Game.Scenes", type.GetProperty("ComponentNamespace").GetValue(contextual));
        var derived = Activator.CreateInstance(type, new object[] { null, project, "Game.Actors", parent1 });
        Assert.Same(parent1, type.GetProperty("ParentComponent").GetValue(derived));
        Assert.Equal(true, type.GetProperty("ComponentUpdatable").GetValue(derived));
        Assert.Equal(roots[0].FullName, type.GetProperty("ComponentBaseType").GetValue(derived));

        var editable = Activator.CreateInstance(type, new object[] { null, project, null, roots[0] });
        var namespaceProperty = type.GetProperty("ComponentNamespace");
        namespaceProperty.SetValue(editable, "Game.Custom");
        namespaceProperty.SetValue(editable, "Game.Entities");
        type.GetProperty("SelectedRoot").SetValue(editable, roots[1]);
        Assert.Equal("Game.Entities", namespaceProperty.GetValue(editable));
    }

    [Fact]
    public void NamespaceFolder_MustBeInsideTheProjectSource()
    {
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.ViewModel.Dialogs.Project.CreateComponentTypeBaseViewModel", true);
        var method = type.GetMethod("TryGetNamespace", BindingFlags.Static | BindingFlags.NonPublic);
        var root = Path.Combine(Path.GetTempPath(), "Game");
        var inside = new object[] { root, Path.Combine(root, "Actors", "Enemies"), "Game", null };
        Assert.True((bool)method.Invoke(null, inside));
        Assert.Equal("Game.Actors.Enemies", inside[3]);
        var rootArgs = new object[] { root, root, "Game", null };
        Assert.True((bool)method.Invoke(null, rootArgs));
        Assert.Equal("Game", rootArgs[3]);
        var outside = new object[] { root, root + "-other", "Game", null };
        Assert.False((bool)method.Invoke(null, outside));
        Assert.Null(outside[3]);
    }

    [Fact]
    public void InheritedCapabilities_AreRecognizedAcrossTheWholeBaseChain()
    {
        var root = CoreComponent.EntityComponent();
        var ancestor = new Component { Namespace = "Game", ClassName = "Actor", BaseInheritedComponent = root };
        ancestor.Interfaces.Add(typeof(IUpdatable));
        ancestor.Interfaces.Add(typeof(IDrawable));
        var middle = new Component { Namespace = "Game", ClassName = "Enemy", BaseInheritedComponent = ancestor };
        var child = new Component { Namespace = "Game", ClassName = "Boss", BaseInheritedComponent = middle };
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.ViewModel.Dialogs.Project.CreateComponentTypeViewModel", true);
        var viewModel = Activator.CreateInstance(type, new object[] { null, MakeProject(), "Game", middle });
        Assert.Equal(true, type.GetProperty("ComponentUpdatable").GetValue(viewModel));
        Assert.Equal(false, type.GetProperty("ComponentDrawableEnabled").GetValue(viewModel));
        Assert.Equal("Inherited from Enemy.", type.GetProperty("DrawableDescription").GetValue(viewModel));
        var code = new TestEntityCode(child);
        var (source, generated) = code.GenerateForTest();
        Assert.Contains("public override void Update(double dt)", source);
        Assert.Contains("public override void Draw(ICanvas canvas)", source);
        Assert.DoesNotContain("public bool Pause", generated);
        Assert.DoesNotContain("public bool Hidden", generated);
    }

    [Fact]
    public void CreationValidation_ShowsQualifiedNameAndChecksNamespacesAndPendingDuplicates()
    {
        var project = MakeProject();
        var existing = new Component { Namespace = "Game.Actors", ClassName = "Player" };
        project.ComponentTree.AddNode("Player", existing.FullName, existing);
        var type = typeof(CoreComponent).Assembly.GetType("TypeDCore.ViewModel.Dialogs.Project.CreateComponentTypeViewModel", true);
        var viewModel = Activator.CreateInstance(type, new object[] { null, project, "Actors", CoreComponent.EntityComponent() });
        object Get(string name) => type.GetProperty(name).GetValue(viewModel);
        void Set(string name, object value) => type.GetProperty(name).SetValue(viewModel, value);
        Assert.Contains("Game.Actors", (IReadOnlyList<string>)Get("NamespaceSuggestions"));
        Set("ComponentName", "Player");
        Assert.Equal("Game.Actors.Player", Get("ResultName"));
        Assert.Contains("already exists", (string)Get("ValidationError"));
        Set("ComponentName", "My-Actor");
        Assert.Contains("valid C# component name", (string)Get("ValidationError"));
        Set("ComponentName", "@class");
        Assert.Contains("valid C# component name", (string)Get("ValidationError"));
        if (OperatingSystem.IsWindows())
        {
            Set("ComponentName", "CON");
            Assert.Contains("file name", (string)Get("ValidationError"));
        }
        Set("ComponentName", "Actor");
        Set("ComponentNamespace", "Actors.invalid-name");
        Assert.Contains("valid C# namespace", (string)Get("ValidationError"));
        Set("ComponentNamespace", "Gameplay");
        Assert.Equal("Game.Gameplay.Actor", Get("ResultName"));
        Assert.True((bool)type.GetMethod("Validate").Invoke(viewModel, null));
    }

    [Fact]
    public void ExactParentFilter_DoesNotMatchOtherRootNamesContainingTheSelectedName()
    {
        var filter = new FilterHelper { Filters = CoreComponent.EntityComponent().FullName, ExactMatch = true };
        Assert.False(filter.Filter(CoreComponent.EntityComponent().FullName));
        Assert.True(filter.Filter(CoreComponent.EntityComponent().FullName + "Extra"));
        Assert.True(filter.Filter(CoreComponent.EntityComponent().FullName.ToLowerInvariant()));
        Assert.True(filter.Filter(CoreComponent.SceneComponent().FullName));
        Assert.False(new FilterHelper { Filters = "Entity" }.Filter(CoreComponent.EntityComponent().FullName));
    }

    [Fact]
    public void CreationMenus_UseOneActionAndPreserveBrowserNamespace()
    {
        var initializerType = typeof(CoreComponent).Assembly.GetType("TypeDCore.TypeDCoreInitializer", true);
        var initializer = Activator.CreateInstance(initializerType, true);
        var registry = DispatchProxy.Create<IEditorCommandRegistry, CallbackProxy>();
        var commands = new List<(string Id, object Data)>();
        ((CallbackProxy)registry).Callback = (method, args) =>
        {
            commands.Add(((string)args[0], args[1]));
            return true;
        };
        initializerType.GetProperty("EditorCommandRegistry", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(initializer, registry);
        var selection = DispatchProxy.Create<INodeSelectionModel, CallbackProxy>();
        ((CallbackProxy)selection).Callback = (method, args) => null;
        initializerType.GetProperty("NodeSelectionModel", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(initializer, selection);
        var componentModel = DispatchProxy.Create<IComponentModel, CallbackProxy>();
        ((CallbackProxy)componentModel).Callback = (method, args) => false;
        initializerType.GetProperty("ComponentModel", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(initializer, componentModel);
        var resources = DispatchProxy.Create<IResourceModel, CallbackProxy>();
        var clipboard = DispatchProxy.Create<INodeClipboardModel, CallbackProxy>();
        ((CallbackProxy)clipboard).Callback = (method, args) => false;
        ((CallbackProxy)resources).Callback = (method, args) => clipboard;
        initializerType.GetProperty("Resources").SetValue(initializer, resources);
        var project = (Project)RuntimeHelpers.GetUninitializedObject(typeof(Project));

        void CheckMenu(MenuItem item, string expectedNamespace)
        {
            Assert.Empty(item.Items);
            item.Click(project);
            var command = commands[^1];
            Assert.Equal("component.create", command.Id);
            Assert.Equal(expectedNamespace, command.Data.GetType().GetProperty("Namespace").GetValue(command.Data));
        }

        var init = new InitUIHook(project);
        initializerType.GetMethod("InitUI", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(initializer, new object[] { init });
        var projectMenu = Assert.Single(init.Menu.Items, item => item.Name == "_Project");
        CheckMenu(Assert.Single(projectMenu.Items, item => item.Name == "_Create Component"), null);

        var tree = new Tree();
        tree.AddNode("Game", "Game", null);
        var game = tree.Get("Game");
        game.AddNode("Actors", "Actors", null);
        var folder = game.Get("Actors");
        var existing = new Component { ClassName = "Existing", Namespace = "Game.Actors", BaseInheritedComponent = CoreComponent.EntityComponent() };
        folder.AddNode("Existing", "Existing", existing);
        var browser = new ComponentTypeBrowserContextMenuOpenedHook(folder);
        initializerType.GetMethod("ComponentTypeBrowserContextMenuOpened", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(initializer, new object[] { browser });
        CheckMenu(Assert.Single(browser.Menu.Items, item => item.Name == "_Create Component"), "Game.Actors");
        var componentBrowser = new ComponentTypeBrowserContextMenuOpenedHook(folder.Get("Existing"));
        initializerType.GetMethod("ComponentTypeBrowserContextMenuOpened", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(initializer, new object[] { componentBrowser });
        CheckMenu(Assert.Single(componentBrowser.Menu.Items, item => item.Name == "Create _Derived Component"), "Game.Actors");
        Assert.Same(existing, commands[^1].Data.GetType().GetProperty("BaseComponent").GetValue(commands[^1].Data));

        var context = new ComponentContextMenuHook { Project = project, Menu = new Menu() };
        initializerType.GetMethod("ComponentContextMenuOpened", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(initializer, new object[] { context });
        CheckMenu(Assert.Single(context.Menu.Items, item => item.Name == "_Create Component"), null);
    }

    [Theory]
    [InlineData("Entity", "CreateEntity", true, false, true)]
    [InlineData("Scene", "CreateScene", false, false, true)]
    [InlineData("Drawable", "CreateDrawable", true, false, true)]
    [InlineData("Entity", "CreateEntity", true, false, false)]
    [InlineData("Entity", "CreateEntity", true, true, true)]
    [InlineData("Drawable", "CreateDrawable", true, true, true)]
    public async Task Creation_DispatchesSelectedRootAndReplaysHistory(string selected, string method, bool firesHook, bool handled, bool remember)
    {
        var initial = CoreComponent.EntityComponent();
        var root = selected == "Scene" ? CoreComponent.SceneComponent()
            : selected == "Drawable" ? CoreComponent.DrawableComponent() : initial;
        var options = new ComponentCreationOptions
        {
            Name = "Example", Namespace = "Custom", ParentComponent = root, BaseType = root.FullName,
            Icon = "icon.png", Updatable = true, Drawable = true, RememberFields = remember
        };
        var calls = new List<string>();
        var dialog = DispatchProxy.Create<IEditorDialogModel, CallbackProxy>();
        ((CallbackProxy)dialog).Callback = (m, args) =>
        {
            Assert.Equal(initial.FullName, ((Component)args[2]).FullName);
            return Task.FromResult(options);
        };
        var model = DispatchProxy.Create<ITypeDCoreProjectModel, CallbackProxy>();
        ((CallbackProxy)model).Callback = (m, args) =>
        {
            calls.Add(m.Name);
            Assert.Equal("Example", args[1]);
            Assert.Equal("Custom", args[2]);
            Assert.Same(root, args[3]);
            Assert.Equal("icon.png", args[^2]);
            Assert.Empty((List<CustomProperty>)args[^1]);
            if (m.Name == "CreateEntity")
            {
                Assert.Equal(true, args[4]);
                Assert.Equal(true, args[5]);
            }
            return new Component { ClassName = "Example", Namespace = "Custom" };
        };
        var provider = DispatchProxy.Create<IComponentProvider, CallbackProxy>();
        ((CallbackProxy)provider).Callback = (m, args) => { calls.Add(m.Name); return null; };
        var historyType = typeof(Component).Assembly.GetType("TypeD.Models.CommandHistoryModel", true);
        var history = (ICommandHistoryModel)Activator.CreateInstance(historyType);
        var hookType = typeof(Component).Assembly.GetType("TypeD.Models.HookModel", true);
        var hooks = (IHookModel)Activator.CreateInstance(hookType);
        var hookCount = 0;
        hooks.AddHook<CreateComponentHook>(hook => { hookCount++; hook.Handled = handled; });
        var creationSettings = new ComponentCreationSettingContext();
        creationSettings.Updatable.Value = false;
        creationSettings.Drawable.Value = false;
        var savedSettings = 0;
        var settings = DispatchProxy.Create<ISettingModel, CallbackProxy>();
        ((CallbackProxy)settings).Callback = (m, args) =>
        {
            if (m.Name == nameof(ISettingModel.GetContext)) return creationSettings;
            if (m.Name == nameof(ISettingModel.SetContext)) { savedSettings++; return null; }
            throw new InvalidOperationException(m.Name);
        };
        var resources = DispatchProxy.Create<IResourceModel, CallbackProxy>();
        ((CallbackProxy)resources).Callback = (m, args) =>
        {
            var service = m.GetGenericArguments()[0];
            if (service == typeof(IEditorDialogModel)) return dialog;
            if (service == typeof(ITypeDCoreProjectModel)) return model;
            if (service == typeof(IComponentProvider)) return provider;
            if (service == typeof(ICommandHistoryModel)) return history;
            if (service == typeof(IHookModel)) return hooks;
            if (service == typeof(ISettingModel)) return settings;
            throw new InvalidOperationException(service.Name);
        };
        var project = (Project)RuntimeHelpers.GetUninitializedObject(typeof(Project));
        var assembly = typeof(CoreComponent).Assembly;
        var dataType = assembly.GetType("TypeDCore.Commands.Data.CreateComponentCommandData", true);
        var data = Activator.CreateInstance(dataType, new object[] { project, null });
        var commandType = assembly.GetType("TypeDCore.Commands.CreateComponentTypeCommand", true);
        var execute = commandType.GetMethod("ExecuteAsync", BindingFlags.Static | BindingFlags.NonPublic);
        await (Task)execute.Invoke(null, new object[] { data, initial, resources });

        Assert.Equal(firesHook ? 1 : 0, hookCount);
        if (handled)
        {
            Assert.Empty(calls);
            Assert.False(history.CanUndo);
            Assert.Equal(0, savedSettings);
            return;
        }
        Assert.Equal(new[] { method }, calls);
        Assert.Equal(remember, creationSettings.RememberFields.Value);
        if (remember)
        {
            Assert.Equal(root.FullName, creationSettings.LastBaseTypes.Value[root.FullName]);
            Assert.Equal(root.FullName, creationSettings.RootType.Value);
            Assert.Equal("Custom", creationSettings.Namespace.Value);
            Assert.Equal("icon.png", creationSettings.Icon.Value);
            Assert.Equal(selected == "Entity", creationSettings.Updatable.Value);
            Assert.Equal(selected == "Entity", creationSettings.Drawable.Value);
        }
        else
            Assert.Empty(creationSettings.LastBaseTypes.Value);
        Assert.Equal(1, savedSettings);
        Assert.True(history.Undo());
        Assert.True(history.Redo());
        Assert.Equal(new[] { method, "Delete", method }, calls);
        Assert.Equal(firesHook ? 1 : 0, hookCount);
    }

    [Fact]
    public async Task CancelledComponentCreation_DoesNotUpdatePreferences()
    {
        var dialog = DispatchProxy.Create<IEditorDialogModel, CallbackProxy>();
        ((CallbackProxy)dialog).Callback = (method, args) => Task.FromResult<ComponentCreationOptions>(null);
        var resources = DispatchProxy.Create<IResourceModel, CallbackProxy>();
        ((CallbackProxy)resources).Callback = (method, args) =>
        {
            if (method.GetGenericArguments()[0] == typeof(IEditorDialogModel)) return dialog;
            throw new InvalidOperationException("Cancellation should not access other services.");
        };
        var assembly = typeof(CoreComponent).Assembly;
        var dataType = assembly.GetType("TypeDCore.Commands.Data.CreateComponentCommandData", true);
        var data = Activator.CreateInstance(dataType, new object[] { MakeProject(), null });
        var command = assembly.GetType("TypeDCore.Commands.CreateComponentTypeCommand", true);
        await (Task)command.GetMethod("ExecuteAsync", BindingFlags.Static | BindingFlags.NonPublic)
            .Invoke(null, new object[] { data, null, resources });
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public async Task AddProperty_OnlyUpdatesPreferencesAfterSuccessfulCreation(int choice)
    {
        var property = new CustomProperty
        {
            Name = "Health", TypeName = typeof(int).FullName, DefaultValue = 42,
            Description = "Hit points", Group = "Stats", Access = CustomPropertyAccess.Public
        };
        (CustomProperty Property, Type Type, bool RememberFields)? result = choice < 0
            ? null : (property, typeof(int), choice == 1);
        var creationSettings = new PropertyCreationSettingContext();
        creationSettings.RememberFields.Value = true;
        creationSettings.Group.Value = "Previous";
        var savedSettings = 0;
        var settings = DispatchProxy.Create<ISettingModel, CallbackProxy>();
        ((CallbackProxy)settings).Callback = (method, args) =>
        {
            if (method.Name == nameof(ISettingModel.GetContext)) return creationSettings;
            if (method.Name == nameof(ISettingModel.SetContext)) { savedSettings++; return null; }
            throw new InvalidOperationException(method.Name);
        };
        var dialog = DispatchProxy.Create<IEditorDialogModel, CallbackProxy>();
        ((CallbackProxy)dialog).Callback = (method, args) => Task.FromResult(result);
        var properties = DispatchProxy.Create<IPropertyModel, CallbackProxy>();
        ((CallbackProxy)properties).Callback = (method, args) => Array.Empty<IPropertyDefinition>();
        var provider = DispatchProxy.Create<IComponentProvider, CallbackProxy>();
        ((CallbackProxy)provider).Callback = (method, args) => null;
        var projects = DispatchProxy.Create<IProjectModel, CallbackProxy>();
        ((CallbackProxy)projects).Callback = (method, args) => null;
        var registry = DispatchProxy.Create<IEditorCommandRegistry, CallbackProxy>();
        ((CallbackProxy)registry).Callback = (method, args) => null;
        var selection = DispatchProxy.Create<INodeSelectionModel, CallbackProxy>();
        var log = DispatchProxy.Create<ILogModel, CallbackProxy>();
        var hooks = (IHookModel)Activator.CreateInstance(typeof(Component).Assembly.GetType("TypeD.Models.HookModel", true));
        var history = (ICommandHistoryModel)Activator.CreateInstance(typeof(Component).Assembly.GetType("TypeD.Models.CommandHistoryModel", true));
        var resources = DispatchProxy.Create<IResourceModel, CallbackProxy>();
        ((CallbackProxy)resources).Callback = (method, args) =>
        {
            var service = method.GetGenericArguments()[0];
            if (service == typeof(ISettingModel)) return settings;
            if (service == typeof(IEditorDialogModel)) return dialog;
            if (service == typeof(IPropertyModel)) return properties;
            if (service == typeof(IComponentProvider)) return provider;
            if (service == typeof(IProjectModel)) return projects;
            if (service == typeof(IEditorCommandRegistry)) return registry;
            if (service == typeof(INodeSelectionModel)) return selection;
            if (service == typeof(ILogModel)) return log;
            if (service == typeof(IHookModel)) return hooks;
            if (service == typeof(ICommandHistoryModel)) return history;
            throw new InvalidOperationException(service.Name);
        };
        var assembly = typeof(CoreComponent).Assembly;
        var resourceProperty = typeof(ViewModelBase).GetProperty(nameof(ViewModelBase.ResourceModel));
        var previousResources = resourceProperty.GetValue(null);
        resourceProperty.SetValue(null, resources);
        try
        {
            var commandType = assembly.GetType("TypeDCore.Commands.AddCustomPropertyCommand", true);
            var command = Activator.CreateInstance(commandType, new object[] { resources });
            var dataType = assembly.GetType("TypeDCore.Commands.Data.ComponentCommandData", true);
            var data = Activator.CreateInstance(dataType);
            var component = CoreComponent.EntityComponent();
            dataType.GetProperty("Project").SetValue(data, MakeProject());
            dataType.GetProperty("Component").SetValue(data, component);
            await (Task)commandType.GetMethod("ExecuteAsync", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(command, new[] { data });

            if (choice < 0)
            {
                Assert.Empty(component.CustomProperties);
                Assert.Equal(0, savedSettings);
                Assert.True(creationSettings.RememberFields.Value);
                Assert.Equal("Previous", creationSettings.Group.Value);
            }
            else
            {
                Assert.Single(component.CustomProperties);
                Assert.Equal(1, savedSettings);
                Assert.Equal(choice == 1, creationSettings.RememberFields.Value);
                Assert.Equal(choice == 1 ? "Stats" : "Previous", creationSettings.Group.Value);
                if (choice == 1)
                {
                    Assert.Equal(typeof(int).FullName, creationSettings.TypeName.Value);
                    Assert.Equal("42", creationSettings.DefaultValueJson.Value);
                    Assert.Equal("Hit points", creationSettings.Description.Value);
                }
            }
        }
        finally
        {
            resourceProperty.SetValue(null, previousResources);
        }
    }

    public class CallbackProxy : DispatchProxy
    {
        public Func<MethodInfo, object[], object> Callback { get; set; }
        protected override object Invoke(MethodInfo targetMethod, object[] args) => Callback(targetMethod, args);
    }

    private sealed class TestEntityCode(Component component) : EntityCode(component)
    {
        public (string Source, string Generated) GenerateForTest()
        {
            Writer.TargetFile = TypeDFile;
            InitTypeDClass();
            Writer.TargetFile = BaseFile;
            InitClass();
            Generate();
            return (BaseFile.Output.ToString(), TypeDFile.Output.ToString());
        }
    }

    public static Project MakeProject()
    {
        var project = (Project)RuntimeHelpers.GetUninitializedObject(typeof(Project));
        typeof(Project).GetProperty(nameof(Project.ProjectName)).SetValue(project, "Game");
        typeof(Project).GetProperty(nameof(Project.Location)).SetValue(project, Path.GetTempPath());
        project.ComponentTree = new Tree();
        return project;
    }
}
