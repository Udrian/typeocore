using System.Text.Json;
using TypeD.Helpers;
using TypeD.Models.Data;

namespace TypeDCore.Commands
{
    internal sealed class AddComponentPropertyPlan
    {
        private sealed record Change(Component Component, CustomProperty Definition, Property Property, Property Baseline);

        private readonly List<Change> Changes;
        private readonly string PropertyName;
        private readonly CustomProperty Original;
        private readonly Type ValueType;
        private readonly string DefaultJson;
        public IReadOnlyList<Component> AffectedRoots { get; }
        public IReadOnlyList<Component> AffectedComponents { get; }

        public AddComponentPropertyPlan(IEnumerable<Component> roots, string declaringType, CustomProperty property, Type valueType)
        {
            ArgumentNullException.ThrowIfNull(property);
            ArgumentNullException.ThrowIfNull(valueType);
            if (string.IsNullOrEmpty(property.Name) || property.TypeName != valueType.FullName)
                throw new ArgumentException("The property name and value type must match its definition.", nameof(property));
            PropertyName = property.Name;
            Original = new CustomProperty(property);
            ValueType = valueType;
            DefaultJson = JsonSerializer.Serialize(property.DefaultValue, valueType, JSON.CreateOptions());
            var rootList = roots.Distinct().ToList();
            var nodes = rootList.SelectMany(RemoveCustomPropertyPlan.Traverse).Distinct().ToList();
            var affected = nodes.Where(node => FindDeclaring(node, declaringType) != null).ToList();
            if (!affected.Any(node => node.FullName == declaringType))
                throw new InvalidOperationException($"Component '{declaringType}' was not found.");
            foreach (var node in affected)
            {
                if (HasConflict(node, property.Name))
                    throw new InvalidOperationException($"'{property.Name}' already exists on {node.FullName}.");
            }

            var json = JsonSerializer.SerializeToElement(property.DefaultValue, valueType, JSON.CreateOptions());
            object CopyValue() => json.Deserialize(valueType, JSON.CreateOptions());
            Changes = affected.Select(node =>
            {
                var declaring = FindDeclaring(node, declaringType);
                var definition = node.FullName == declaringType
                    ? new CustomProperty(property) { DefaultValue = CopyValue() } : null;
                var visible = property.Access == CustomPropertyAccess.Public
                    ? new Property
                    {
                        Name = property.Name,
                        Description = property.Description,
                        Group = property.Group,
                        DeclaringTypeName = declaringType,
                        Type = valueType,
                        Value = CopyValue(),
                        FromComponent = declaring
                    } : null;
                return new Change(node, definition, visible, visible == null ? null : new Property(visible) { Value = CopyValue() });
            }).ToList();
            AffectedComponents = affected;
            var affectedSet = affected.ToHashSet();
            AffectedRoots = rootList.Where(root => RemoveCustomPropertyPlan.Traverse(root).Any(affectedSet.Contains)).ToList();
        }

        public void Apply()
        {
            foreach (var change in Changes)
            {
                if (!Unchanged(change))
                    throw new InvalidOperationException($"The added property '{PropertyName}' changed since the edit was prepared.");
                if (HasConflict(change.Component, PropertyName))
                    throw new InvalidOperationException($"'{PropertyName}' already exists on {change.Component.FullName}.");
            }
            foreach (var change in Changes)
            {
                if (change.Definition != null)
                    change.Component.CustomProperties.Add(change.Definition);
                if (change.Property != null)
                {
                    change.Component.Properties.Add(change.Property);
                    change.Component.OveriddenProperties.Add(change.Baseline);
                }
            }
        }

        public void Undo()
        {
            if (Changes.Any(change => (change.Definition != null && (change.Definition.Name != PropertyName
                    || !change.Component.CustomProperties.Contains(change.Definition)))
                || (change.Property != null && (!change.Component.Properties.Contains(change.Property)
                    || change.Property.Name != PropertyName
                    || !change.Component.OveriddenProperties.Contains(change.Baseline)))
                || !Unchanged(change)))
                throw new InvalidOperationException("The added property changed since the edit was prepared.");
            foreach (var change in Changes)
            {
                if (change.Definition != null)
                    change.Component.CustomProperties.Remove(change.Definition);
                if (change.Property != null)
                {
                    change.Component.Properties.Remove(change.Property);
                    change.Component.OveriddenProperties.Remove(change.Baseline);
                }
            }
        }

        private bool Unchanged(Change change)
        {
            bool ValueMatches(object value) =>
                JsonSerializer.Serialize(value, ValueType, JSON.CreateOptions()) == DefaultJson;
            bool PropertyMatches(Property property) => property == null ||
                (property.Name == Original.Name && property.Type == ValueType
                    && property.Description == Original.Description && property.Group == Original.Group
                    && ValueMatches(property.Value));
            var definition = change.Definition;
            return (definition == null || (definition.Name == Original.Name && definition.TypeName == Original.TypeName
                    && definition.Description == Original.Description && definition.Group == Original.Group
                    && definition.Access == Original.Access && ValueMatches(definition.DefaultValue)))
                && PropertyMatches(change.Property) && PropertyMatches(change.Baseline);
        }

        private static Component FindDeclaring(Component component, string declaringType)
        {
            var visited = new HashSet<Component>();
            for (var current = component; current != null && visited.Add(current); current = current.BaseInheritedComponent)
                if (current.FullName == declaringType)
                    return current;
            return null;
        }

        private static bool HasConflict(Component component, string name)
        {
            if (component.Properties.Any(existing => existing.Name == name))
                return true;
            var visited = new HashSet<Component>();
            for (var current = component; current != null && visited.Add(current); current = current.BaseInheritedComponent)
                if (current.CustomProperties.Any(existing => existing.Name == name))
                    return true;
            return false;
        }
    }
}
