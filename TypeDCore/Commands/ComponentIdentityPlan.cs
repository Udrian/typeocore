using TypeD.Models.Data;
using TypeDCore.Models.Data;
using TypeOEngine.Typedeaf.Core;

namespace TypeDCore.Commands
{
    internal sealed class ComponentIdentityPlan
    {
        private readonly Project Project;
        private readonly string OldName;
        private readonly string NewName;
        private readonly string NewNamespace;
        private readonly string NewClassName;
        private readonly string OldNamespace;
        private readonly string OldClassName;
        private readonly List<Component> Components;
        private readonly List<Property> DeclaringProperties;
        private readonly List<Property> StartingScenes;
        private readonly bool UpdateProjectStartScene;

        public IReadOnlyList<Component> AffectedRoots { get; }
        public IReadOnlyList<Component> AffectedComponents { get; }
        public bool ChangesProject => UpdateProjectStartScene;

        public ComponentIdentityPlan(Project project, IEnumerable<Component> roots, string oldName,
            string newNamespace, string newClassName)
        {
            ArgumentNullException.ThrowIfNull(project);
            ArgumentException.ThrowIfNullOrEmpty(oldName);
            ArgumentException.ThrowIfNullOrEmpty(newNamespace);
            ArgumentException.ThrowIfNullOrEmpty(newClassName);
            Project = project;
            OldName = oldName;
            var separator = oldName.LastIndexOf('.');
            if (separator <= 0 || separator == oldName.Length - 1)
                throw new ArgumentException("The component name must include a namespace.", nameof(oldName));
            OldNamespace = oldName[..separator];
            OldClassName = oldName[(separator + 1)..];
            NewNamespace = newNamespace;
            NewClassName = newClassName;
            NewName = $"{newNamespace}.{newClassName}";
            var rootList = roots.Distinct().ToList();
            var nodes = rootList.SelectMany(RemoveCustomPropertyPlan.Traverse).Distinct().ToList();
            Components = nodes.Where(component => component.FullName == oldName).ToList();
            if (Components.Count == 0)
                throw new InvalidOperationException($"Component '{oldName}' was not found.");
            if (oldName != NewName && nodes.Any(component => component.FullName == NewName))
                throw new InvalidOperationException($"Component '{NewName}' already exists.");

            var properties = nodes.SelectMany(component => component.Properties.Concat(component.OveriddenProperties))
                .Distinct().ToList();
            DeclaringProperties = properties.Where(property => property.DeclaringTypeName == oldName).ToList();
            StartingScenes = nodes.Where(component => component.IsBasedOn(typeof(Game)))
                .SelectMany(component => component.Properties.Concat(component.OveriddenProperties))
                .Where(property => property.Name == StartingScene.PropertyName
                    && property.DeclaringTypeName == typeof(Game).FullName
                    && property.Value is string value && value == oldName)
                .Distinct().ToList();
            UpdateProjectStartScene = project.StartScene == oldName;

            var affected = nodes.Where(component => Components.Contains(component)
                || component.Properties.Concat(component.OveriddenProperties)
                    .Any(property => DeclaringProperties.Contains(property) || StartingScenes.Contains(property))).ToHashSet();
            AffectedComponents = affected.ToList();
            AffectedRoots = rootList.Where(root =>
                RemoveCustomPropertyPlan.Traverse(root).Any(affected.Contains)).ToList();
        }

        public void Apply() => SetIdentity(OldName, NewName, NewNamespace, NewClassName);

        public void Undo() => SetIdentity(NewName, OldName, OldNamespace, OldClassName);

        private void SetIdentity(string expected, string replacement, string componentNamespace, string className)
        {
            if (Components.Any(component => component.FullName != expected)
                || DeclaringProperties.Any(property => property.DeclaringTypeName != expected)
                || StartingScenes.Any(property => !Equals(property.Value, expected))
                || (UpdateProjectStartScene && Project.StartScene != expected))
                throw new InvalidOperationException("The component references changed since the edit was prepared.");

            foreach (var component in Components)
            {
                component.Namespace = componentNamespace;
                component.ClassName = className;
            }
            foreach (var property in DeclaringProperties)
                property.DeclaringTypeName = replacement;
            foreach (var property in StartingScenes)
                property.Value = replacement;
            if (UpdateProjectStartScene)
                Project.StartScene = replacement;
        }
    }
}
