using TypeD.Models.Data;

namespace TypeDCore.Commands
{
    internal sealed class RemoveCustomPropertyPlan
    {
        private sealed class Change
        {
            public Component Component { get; init; }
            public List<(int Index, Property Item)> Properties { get; init; }
            public List<(int Index, Property Item)> Baselines { get; init; }
            public List<(int Index, CustomProperty Item)> CustomProperties { get; init; }
            public bool AddTombstone { get; init; }
        }

        private readonly List<Change> Changes;
        private readonly string PropertyName;

        public IReadOnlyList<Component> AffectedRoots { get; }
        public IReadOnlyList<Component> AffectedComponents => Changes.Select(change => change.Component).ToList();
        internal IEnumerable<(Component Component, Property Property)> PreviewEntries =>
            Changes.SelectMany(change => change.Properties.Select(entry => (change.Component, entry.Item)));

        public RemoveCustomPropertyPlan(IEnumerable<Component> roots, string declaringTypeName, string propertyName)
        {
            PropertyName = propertyName;
            var rootList = roots.Distinct().ToList();
            var components = rootList.SelectMany(Traverse).Distinct().ToList();
            bool Matches(Property property) => property.Name == propertyName
                && (property.DeclaringTypeName ?? property.FromComponent?.FullName) == declaringTypeName;

            Changes = components.Select(component => new Change
            {
                Component = component,
                Properties = component.Properties.Select((item, index) => (Index: index, Item: item))
                    .Where(entry => Matches(entry.Item)).ToList(),
                Baselines = component.OveriddenProperties.Select((item, index) => (Index: index, Item: item))
                    .Where(entry => Matches(entry.Item)).ToList(),
                CustomProperties = component.FullName == declaringTypeName
                    ? component.CustomProperties.Select((item, index) => (Index: index, Item: item))
                        .Where(entry => entry.Item.Name == propertyName).ToList()
                    : new List<(int, CustomProperty)>(),
                AddTombstone = component.FullName == declaringTypeName
                    && !component.RemovedCustomProperties.Contains(propertyName)
            }).Where(change => change.Properties.Count > 0 || change.Baselines.Count > 0
                || change.CustomProperties.Count > 0 || change.AddTombstone).ToList();

            var affected = Changes.Select(change => change.Component).ToHashSet();
            AffectedRoots = rootList.Where(root => Traverse(root).Any(affected.Contains)).ToList();
        }

        public void Apply()
        {
            foreach (var change in Changes)
            {
                foreach (var entry in change.Properties)
                    change.Component.Properties.Remove(entry.Item);
                foreach (var entry in change.Baselines)
                    change.Component.OveriddenProperties.Remove(entry.Item);
                foreach (var entry in change.CustomProperties)
                    change.Component.CustomProperties.Remove(entry.Item);
                if (change.AddTombstone)
                    change.Component.RemovedCustomProperties.Add(PropertyName);
            }
        }

        public void Undo()
        {
            foreach (var change in Changes)
            {
                foreach (var entry in change.Properties)
                    change.Component.Properties.Insert(Math.Min(entry.Index, change.Component.Properties.Count), entry.Item);
                foreach (var entry in change.Baselines)
                    change.Component.OveriddenProperties.Insert(Math.Min(entry.Index, change.Component.OveriddenProperties.Count), entry.Item);
                foreach (var entry in change.CustomProperties)
                    change.Component.CustomProperties.Insert(Math.Min(entry.Index, change.Component.CustomProperties.Count), entry.Item);
                if (change.AddTombstone)
                    change.Component.RemovedCustomProperties.Remove(PropertyName);
            }
        }

        internal static IEnumerable<Component> Traverse(Component root)
        {
            var pending = new Stack<Component>();
            var visited = new HashSet<Component>();
            pending.Push(root);
            while (pending.Count > 0)
            {
                var component = pending.Pop();
                if (!visited.Add(component))
                    continue;
                yield return component;
                if (component.BaseInheritedComponent != null)
                    pending.Push(component.BaseInheritedComponent);
                foreach (var child in component.Children)
                    pending.Push(child);
            }
        }
    }
}
