using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using TypeD.Helpers;
using TypeD.Models.Data;
using TypeD.Models.Data.SaveContexts;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;

namespace TypeDCore.Commands
{
    internal sealed class ComponentIdentityFiles
    {
        private readonly Project Project;
        private readonly IComponentProvider Provider;
        private readonly ComponentSaveContext SaveContext;
        private readonly IReadOnlyList<Component> Roots;
        private readonly Component Target;
        private readonly string OldName;
        private readonly string NewName;
        private readonly bool ChangesProject;
        private readonly PropertyCodeRename Rename;
        private readonly Dictionary<string, byte[]> Before;
        private readonly Dictionary<Component, string> OriginalComponentPaths;
        private readonly Dictionary<Component, string> OriginalGeneratedPaths;
        private readonly string OldSource;
        private readonly string NewSource;
        private readonly string NewComponent;
        private readonly string NewGenerated;

        public ComponentIdentityFiles(Project project, IComponentProvider provider, ISaveModel saveModel,
            IReadOnlyList<Component> roots, Component target, string newName, bool changesProject, PropertyCodeRename rename)
        {
            Project = project;
            Provider = provider;
            SaveContext = saveModel.GetSaveContext<ComponentSaveContext>(project);
            Roots = roots;
            Target = target;
            OldName = target.FullName;
            NewName = newName;
            ChangesProject = changesProject;
            Rename = rename ?? throw new ArgumentNullException(nameof(rename));
            Before = new Dictionary<string, byte[]>(OperatingSystem.IsWindows()
                ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            OriginalComponentPaths = roots.ToDictionary(root => root, root => provider.GetPath(project, root));
            OriginalGeneratedPaths = roots.ToDictionary(root => root, root => root.Template.Code.FilePathTypeD());
            OldSource = SourcePath(OldName);
            NewSource = SourcePath(NewName);
            NewComponent = provider.GetPath(project, NewName);
            NewGenerated = GeneratedPath(NewName);
            foreach (var path in OriginalComponentPaths.Values.Concat(OriginalGeneratedPaths.Values)
                .Concat(new[] { OldSource, NewSource, NewComponent, NewGenerated })
                .Concat(rename.Paths))
                Capture(path);
            if (changesProject)
                Capture(project.ProjectFilePath);
            foreach (var path in OriginalComponentPaths.Values)
                if (Before[Key(path)] == null)
                    throw new IOException($"The component file '{path}' must be saved before changing its identity.");
            CheckDestination(OriginalComponentPaths[target], NewComponent);
            CheckDestination(OriginalGeneratedPaths[target], NewGenerated);
            CheckDestination(OldSource, NewSource);
            foreach (var change in rename.FileChanges)
                if (!Equal(Before[Key(change.Path)], change.Before))
                    throw new IOException($"'{change.Path}' changed while the rename was prepared.");
        }

        public IReadOnlyList<string> Paths => Before.Keys.ToList();

        public ComponentFileChangeSet CreateChangeSet()
        {
            var after = new Dictionary<string, byte[]>(Before, Before.Comparer);
            foreach (var change in Rename.FileChanges)
                after[Key(change.Path)] = change.After;
            var source = after[Key(OldSource)];
            if (source != null)
            {
                if (!SamePath(OldSource, NewSource))
                    after[Key(OldSource)] = null;
                after[Key(NewSource)] = source;
            }

            foreach (var root in Roots)
            {
                var previous = Key(OriginalComponentPaths[root]);
                var current = Key(Provider.GetPath(Project, root));
                if (!SamePath(previous, current))
                    after[previous] = null;
                after[current] = JsonSerializer.SerializeToUtf8Bytes(SaveContext.CreateDocument(root), JSON.CreateOptions());
                var generated = root.Template.Code.GenerateTypeDFile();
                var previousGenerated = Key(OriginalGeneratedPaths[root]);
                var currentGenerated = Key(generated.Path);
                if (!Before.ContainsKey(currentGenerated))
                    throw new InvalidOperationException($"Unexpected generated file path '{generated.Path}'.");
                if (!SamePath(previousGenerated, currentGenerated))
                    after[previousGenerated] = null;
                after[currentGenerated] = Encoding.UTF8.GetBytes(generated.Content);
            }
            if (Target.FullName != NewName)
                throw new InvalidOperationException("The component identity was not applied.");
            if (ChangesProject)
            {
                var path = Key(Project.ProjectFilePath);
                var bytes = Before[path] ?? throw new IOException("The project file does not exist.");
                var json = JsonNode.Parse(bytes)?.AsObject()
                    ?? throw new InvalidDataException("The project file is not a JSON object.");
                if (json["StartScene"]?.GetValue<string>() != OldName)
                    throw new IOException("The project start scene changed since the edit was prepared.");
                json["StartScene"] = NewName;
                after[path] = JsonSerializer.SerializeToUtf8Bytes(json, JSON.CreateOptions());
            }
            return new ComponentFileChangeSet(Project.Location, after
                .Where(entry => !Equal(Before[entry.Key], entry.Value))
                .Select(entry => (entry.Key, Before[entry.Key], entry.Value)));
        }

        private void Capture(string path)
        {
            path = Key(path);
            if (!Before.ContainsKey(path))
                Before.Add(path, File.Exists(path) ? File.ReadAllBytes(path) : null);
        }

        private void CheckDestination(string source, string destination)
        {
            if (!SamePath(source, destination) && Before[Key(destination)] != null)
                throw new IOException($"The destination file already exists: '{destination}'.");
        }

        private string SourcePath(string fullName) =>
            Path.Combine(Project.Location, fullName.Replace('.', Path.DirectorySeparatorChar) + ".cs");
        private string GeneratedPath(string fullName) =>
            Path.Combine(Project.ProjectComponentCodePath, fullName.Replace('.', Path.DirectorySeparatorChar) + ".typed.cs");
        private static string Key(string path) => Path.GetFullPath(path);
        private bool SamePath(string first, string second) => Before.Comparer.Equals(Key(first), Key(second));
        private static bool Equal(byte[] first, byte[] second) =>
            first == null ? second == null : second != null && first.SequenceEqual(second);
    }
}
