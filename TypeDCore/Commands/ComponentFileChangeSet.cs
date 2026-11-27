namespace TypeDCore.Commands
{
    internal sealed class ComponentFileChangeSet
    {
        private sealed record Change(string Path, byte[] Before, byte[] After);
        private readonly List<Change> Changes;
        private readonly string ProjectPath;

        public ComponentFileChangeSet(string projectPath, IEnumerable<(string Path, byte[] Before, byte[] After)> changes)
        {
            ArgumentException.ThrowIfNullOrEmpty(projectPath);
            ArgumentNullException.ThrowIfNull(changes);
            ProjectPath = Path.GetFullPath(projectPath);
            var root = Path.EndsInDirectorySeparator(ProjectPath)
                ? ProjectPath : ProjectPath + Path.DirectorySeparatorChar;
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var comparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            Changes = new List<Change>();
            var paths = new HashSet<string>(comparer);
            foreach (var entry in changes)
            {
                var path = Path.GetFullPath(entry.Path);
                if (!path.StartsWith(root, comparison))
                    throw new InvalidOperationException($"The component change would modify a file outside the project: {path}");
                if (!paths.Add(path))
                    throw new InvalidOperationException($"The component change contains multiple edits to '{path}'.");
                Changes.Add(new Change(path, entry.Before?.ToArray(), entry.After?.ToArray()));
            }
        }

        public IReadOnlyList<string> Paths => Changes.Select(change => change.Path).ToList();
        public void Apply() => WriteChanges(true);
        public void Undo() => WriteChanges(false);

        private void WriteChanges(bool forward)
        {
            foreach (var change in Changes)
            {
                ValidatePath(change.Path);
                var expected = forward ? change.Before : change.After;
                if (!Matches(change.Path, expected))
                    throw new IOException($"'{change.Path}' changed since the component edit was prepared.");
            }

            var written = new List<Change>();
            try
            {
                foreach (var change in Changes)
                {
                    ValidatePath(change.Path);
                    written.Add(change);
                    Write(change.Path, forward ? change.After : change.Before);
                }
            }
            catch (Exception failure)
            {
                var rollbackFailures = new List<Exception>();
                foreach (var change in written.AsEnumerable().Reverse())
                {
                    try
                    {
                        ValidatePath(change.Path);
                        Write(change.Path, forward ? change.Before : change.After);
                    }
                    catch (Exception rollbackFailure)
                    {
                        rollbackFailures.Add(rollbackFailure);
                    }
                }
                if (rollbackFailures.Count != 0)
                    throw new AggregateException("The component edit failed and some files could not be restored.",
                        new[] { failure }.Concat(rollbackFailures));
                throw;
            }
        }

        private void ValidatePath(string path)
        {
            var current = ProjectPath;
            CheckReparsePoint(current);
            foreach (var part in Path.GetRelativePath(ProjectPath, path).Split(
                new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries))
            {
                current = Path.Combine(current, part);
                CheckReparsePoint(current);
            }
        }

        private static void CheckReparsePoint(string path)
        {
            try
            {
                if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                    throw new IOException($"The component change cannot follow a link: '{path}'.");
            }
            catch (FileNotFoundException) { }
            catch (DirectoryNotFoundException) { }
        }

        private static bool Matches(string path, byte[] expected)
        {
            if (Directory.Exists(path))
                throw new IOException($"'{path}' is a directory, not a component file.");
            if (!File.Exists(path))
                return expected == null;
            return expected != null && File.ReadAllBytes(path).SequenceEqual(expected);
        }

        private static void Write(string path, byte[] content)
        {
            if (content == null)
            {
                if (File.Exists(path))
                    File.Delete(path);
                else if (Directory.Exists(path))
                    throw new IOException($"'{path}' is a directory, not a component file.");
                return;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, content);
        }
    }
}
