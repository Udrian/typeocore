using System.Text;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        internal IEnumerable<(string Path, byte[] Before, byte[] After)> FileChanges =>
            Changes.Select(change => (change.Path, change.Before.ToArray(), change.After.ToArray()));

        public static Task<PropertyCodeRename> CreateAsync(Project project, string declaringType, string oldName, string newName) =>
            CreateComponentAsync(project, declaringType, new[] { (oldName, newName) }, null);

        public static async Task<PropertyCodeRename> CreateComponentAsync(Project project, string declaringType,
            IReadOnlyList<(string OldName, string NewName)> propertyRenames, string newClassName, string newNamespace = null)
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
            var oldSolution = game.Solution;
            var renamed = await RenameSymbolsAsync(game, declaringType, propertyRenames, newClassName);
            if (newNamespace != null)
            {
                var separator = declaringType.LastIndexOf('.');
                if (separator <= 0)
                    throw new InvalidOperationException("The component must have a namespace.");
                var currentName = declaringType[..(separator + 1)] +
                    (string.IsNullOrEmpty(newClassName) ? declaringType[(separator + 1)..] : newClassName);
                renamed = await MoveComponentNamespaceAsync(renamed.GetProject(game.Id), currentName, newNamespace);
            }
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

        internal static async Task<Solution> RenameSymbolsAsync(Microsoft.CodeAnalysis.Project project, string declaringType,
            IReadOnlyList<(string OldName, string NewName)> propertyRenames, string newClassName)
        {
            ArgumentNullException.ThrowIfNull(propertyRenames);
            var compilation = await project.GetCompilationAsync()
                ?? throw new InvalidOperationException("Could not compile the project for semantic rename.");
            var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Take(5).ToList();
            if (errors.Count != 0)
                throw new InvalidOperationException("Fix the project build errors before renaming: " + string.Join("; ", errors));
            var type = compilation.GetTypeByMetadataName(declaringType)
                ?? throw new InvalidOperationException($"Could not find '{declaringType}' in the project. Save the component and build the project before renaming.");
            var renames = propertyRenames.Where(rename => rename.OldName != rename.NewName).ToList();
            if (renames.Select(rename => rename.OldName).Distinct(StringComparer.Ordinal).Count() != renames.Count ||
                renames.Select(rename => rename.NewName).Distinct(StringComparer.Ordinal).Count() != renames.Count)
                throw new InvalidOperationException("Property renames must have distinct original and replacement names.");
            var temporary = new List<(string Name, string Replacement)>();
            var solution = project.Solution;
            foreach (var rename in renames)
            {
                var symbol = type.GetMembers(rename.OldName).OfType<IPropertySymbol>().SingleOrDefault()
                    ?? throw new InvalidOperationException($"Could not find '{declaringType}.{rename.OldName}' in the project. Save the component and build the project before renaming.");
                string temporaryName;
                do
                    temporaryName = "__TypeDRename_" + Guid.NewGuid().ToString("N");
                while (type.GetMembers(temporaryName).Length != 0);
                solution = await Renamer.RenameSymbolAsync(solution, symbol, new SymbolRenameOptions(), temporaryName);
                temporary.Add((temporaryName, rename.NewName));
                compilation = await solution.GetProject(project.Id).GetCompilationAsync()
                    ?? throw new InvalidOperationException("Could not compile the project after preparing a rename.");
                type = compilation.GetTypeByMetadataName(declaringType)
                    ?? throw new InvalidOperationException($"Could not find '{declaringType}' after preparing a rename.");
            }
            foreach (var rename in temporary)
            {
                var symbol = type.GetMembers(rename.Name).OfType<IPropertySymbol>().SingleOrDefault()
                    ?? throw new InvalidOperationException($"Could not find the temporary property '{rename.Name}'.");
                solution = await Renamer.RenameSymbolAsync(solution, symbol, new SymbolRenameOptions(), rename.Replacement);
                compilation = await solution.GetProject(project.Id).GetCompilationAsync()
                    ?? throw new InvalidOperationException("Could not compile the project after preparing a rename.");
                type = compilation.GetTypeByMetadataName(declaringType)
                    ?? throw new InvalidOperationException($"Could not find '{declaringType}' after preparing a rename.");
            }
            if (!string.IsNullOrEmpty(newClassName) && newClassName != type.Name)
                solution = await Renamer.RenameSymbolAsync(solution, type, new SymbolRenameOptions(), newClassName);
            return solution;
        }

        internal static async Task<Solution> MoveComponentNamespaceAsync(Microsoft.CodeAnalysis.Project project,
            string declaringType, string newNamespace)
        {
            ArgumentNullException.ThrowIfNull(project);
            ArgumentException.ThrowIfNullOrEmpty(newNamespace);
            if (!SyntaxFacts.IsValidIdentifier(newNamespace.Split('.')[0]) ||
                newNamespace.Split('.').Any(part => !SyntaxFacts.IsValidIdentifier(part)))
                throw new ArgumentException("The destination namespace is invalid.", nameof(newNamespace));
            var compilation = await project.GetCompilationAsync()
                ?? throw new InvalidOperationException("Could not compile the project for namespace migration.");
            var type = compilation.GetTypeByMetadataName(declaringType)
                ?? throw new InvalidOperationException($"Could not find '{declaringType}' for namespace migration.");
            if (type.Arity != 0 || type.ContainingType != null || type.ContainingNamespace.IsGlobalNamespace)
                throw new InvalidOperationException("Only non-generic top-level component types in a namespace can be moved.");
            var oldNamespace = type.ContainingNamespace.ToDisplayString();
            if (oldNamespace == newNamespace)
                return project.Solution;
            if (compilation.GetTypeByMetadataName($"{newNamespace}.{type.Name}") != null)
                throw new InvalidOperationException($"A type named '{newNamespace}.{type.Name}' already exists.");

            var namespaces = new Dictionary<DocumentId, HashSet<SyntaxNode>>();
            foreach (var syntaxReference in type.DeclaringSyntaxReferences)
            {
                var declaration = await syntaxReference.GetSyntaxAsync() as ClassDeclarationSyntax;
                var document = project.Solution.GetDocument(syntaxReference.SyntaxTree);
                if (document == null || declaration?.Parent is not BaseNamespaceDeclarationSyntax container ||
                    container.Parent is not CompilationUnitSyntax ||
                    container.Members.Any(member => member is not ClassDeclarationSyntax candidate ||
                        candidate.Identifier.ValueText != type.Name || candidate.TypeParameterList != null))
                    throw new InvalidOperationException(
                        $"Cannot safely move '{declaringType}': each partial declaration must be the only type in a top-level namespace.");
                if (!namespaces.TryGetValue(document.Id, out var set))
                    namespaces.Add(document.Id, set = new HashSet<SyntaxNode>());
                set.Add(container);
            }
            if (namespaces.Count == 0)
                throw new InvalidOperationException($"No source declarations of '{declaringType}' were found.");

            var solution = project.Solution;
            var replacementName = $"global::{newNamespace}.{type.Name}";
            foreach (var document in project.Solution.Projects.SelectMany(item => item.Documents))
            {
                var root = await document.GetSyntaxRootAsync();
                var model = await document.GetSemanticModelAsync();
                if (root == null || model == null)
                    throw new InvalidOperationException($"Could not inspect '{document.Name}' for namespace migration.");
                var referencedType = model.Compilation.GetTypeByMetadataName(declaringType);
                if (referencedType != null && !referencedType.ContainingAssembly.Identity.Equals(type.ContainingAssembly.Identity))
                    referencedType = null;
                var references = new HashSet<SyntaxNode>();
                foreach (var identifier in root.DescendantNodes().OfType<IdentifierNameSyntax>())
                {
                    if (identifier.Identifier.ValueText != type.Name || referencedType == null ||
                        !SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(identifier).Symbol, referencedType))
                        continue;
                    SyntaxNode reference = identifier;
                    while (reference.Parent is NameSyntax or MemberAccessExpressionSyntax)
                    {
                        if (!SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(reference.Parent).Symbol, referencedType))
                            break;
                        reference = reference.Parent;
                    }
                    references.Add(reference);
                }
                namespaces.TryGetValue(document.Id, out var declarationNamespaces);
                var changes = references.Concat(declarationNamespaces ?? Enumerable.Empty<SyntaxNode>()).ToList();
                if (changes.Count == 0)
                    continue;
                var rewritten = root.ReplaceNodes(changes, (original, current) =>
                {
                    if (declarationNamespaces?.Contains(original) == true)
                        return ((BaseNamespaceDeclarationSyntax)current).WithName(
                            SyntaxFactory.ParseName(newNamespace).WithTriviaFrom(((BaseNamespaceDeclarationSyntax)current).Name));
                    if (current is ExpressionSyntax && current is not NameSyntax)
                        return SyntaxFactory.ParseExpression(replacementName).WithTriviaFrom(current);
                    return SyntaxFactory.ParseName(replacementName).WithTriviaFrom(current);
                });
                solution = solution.WithDocumentSyntaxRoot(document.Id, rewritten);
            }
            return solution;
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
