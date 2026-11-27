using System.Text;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Rename;
using Project = TypeD.Models.Data.Project;

namespace TypeDCore.Commands
{
    internal sealed class PropertyCodeRename
    {
        private sealed record FileChange(string Path, byte[] Before, byte[] After);
        private readonly List<FileChange> Changes;

        private PropertyCodeRename(List<FileChange> changes) => Changes = changes;
        public IReadOnlyList<string> Paths => Changes.Select(change => change.Path).ToList();

        public static async Task<PropertyCodeRename> CreateAsync(Project project, string declaringType, string oldName, string newName)
        {
            var failures = new ConcurrentQueue<string>();
            using var workspace = MSBuildWorkspace.Create(new Dictionary<string, string> { ["TypeODependenciesResolved"] = "true" });
            using var registration = workspace.RegisterWorkspaceFailedHandler(args =>
            {
                if (args.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                    failures.Enqueue(args.Diagnostic.Message);
            });
            var game = await workspace.OpenProjectAsync(project.ProjectCSProjPath);
            if (failures.Count != 0)
                throw new InvalidOperationException("Could not load the project for semantic rename: " + string.Join("; ", failures));
            var compilation = await game.GetCompilationAsync()
                ?? throw new InvalidOperationException("Could not compile the project for semantic rename.");
            var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Take(5).ToList();
            if (errors.Count != 0)
                throw new InvalidOperationException("Fix the project build errors before renaming this property: " + string.Join("; ", errors));
            var type = compilation.GetTypeByMetadataName(declaringType)
                ?? throw new InvalidOperationException($"Could not find '{declaringType}' in the project. Save the component and build the project before renaming.");
            var symbol = type.GetMembers(oldName).OfType<IPropertySymbol>().SingleOrDefault()
                ?? throw new InvalidOperationException($"Could not find '{declaringType}.{oldName}' in the project. Save the component and build the project before renaming.");
            var oldSolution = game.Solution;
            var renamed = await Renamer.RenameSymbolAsync(oldSolution, symbol, new SymbolRenameOptions(), newName);
            var changes = new List<FileChange>();
            var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(project.Location)) + Path.DirectorySeparatorChar;
            var generated = Path.TrimEndingDirectorySeparator(Path.GetFullPath(project.ProjectTypeOPath)) + Path.DirectorySeparatorChar;
            foreach (var projectChange in renamed.GetChanges(oldSolution).GetProjectChanges())
            foreach (var documentId in projectChange.GetChangedDocuments())
            {
                var beforeDocument = oldSolution.GetDocument(documentId);
                var afterDocument = renamed.GetDocument(documentId);
                var path = beforeDocument?.FilePath;
                if (path == null || afterDocument == null)
                    throw new InvalidOperationException("A renamed document has no file path.");
                path = Path.GetFullPath(path);
                if (path.StartsWith(generated, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"The rename would modify a file outside the project: {path}");
                var beforeText = await beforeDocument.GetTextAsync();
                var afterText = await afterDocument.GetTextAsync();
                var bytes = await File.ReadAllBytesAsync(path);
                var encoding = beforeText.Encoding ?? new UTF8Encoding(false);
                using var reader = new StreamReader(new MemoryStream(bytes), encoding, true);
                if (await reader.ReadToEndAsync() != beforeText.ToString())
                    throw new IOException($"'{path}' changed while the rename was prepared.");
                var body = encoding.GetBytes(afterText.ToString());
                var preamble = encoding.GetPreamble();
                var hasPreamble = preamble.Length > 0 && bytes.AsSpan().StartsWith(preamble);
                var result = hasPreamble ? preamble.Concat(body).ToArray() : body;
                var existing = changes.FirstOrDefault(change => string.Equals(change.Path, path, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    if (!existing.Before.SequenceEqual(bytes) || !existing.After.SequenceEqual(result))
                        throw new InvalidOperationException($"The file '{path}' has conflicting changes in different projects.");
                    continue;
                }
                changes.Add(new FileChange(path, bytes, result));
            }
            return new PropertyCodeRename(changes);
        }

        public void Apply() => WriteChanges(redo: true);
        public void Undo() => WriteChanges(redo: false);

        private void WriteChanges(bool redo)
        {
            foreach (var change in Changes)
            {
                if (!File.ReadAllBytes(change.Path).SequenceEqual(redo ? change.Before : change.After))
                    throw new IOException($"'{change.Path}' was modified since the property rename was prepared.");
            }

            var written = new List<FileChange>();
            try
            {
                foreach (var change in Changes)
                {
                    written.Add(change);
                    File.WriteAllBytes(change.Path, redo ? change.After : change.Before);
                }
            }
            catch
            {
                foreach (var change in written)
                    File.WriteAllBytes(change.Path, redo ? change.Before : change.After);
                throw;
            }
        }
    }
}
