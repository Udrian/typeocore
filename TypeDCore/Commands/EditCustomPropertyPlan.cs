using System.Globalization;
using TypeD.Models.Data;

namespace TypeDCore.Commands
{
    internal sealed class EditCustomPropertyPlan
    {
        internal sealed class ValueChange
        {
            public Component Component { get; init; }
            public Property Property { get; init; }
            public bool IsBaseline { get; init; }
            public object Replacement { get; set; }
            public ValueChange FollowValue { get; init; }
        }

        private sealed class PropertyChange
        {
            public Component Component { get; init; }
            public Property Property { get; init; }
            public Property Original { get; init; }
            public bool Baseline { get; init; }
            public int Index { get; init; }
            public ValueChange Value { get; init; }
        }

        private readonly List<PropertyChange> Properties = new();
        private readonly List<(Component Component, CustomProperty Property, CustomProperty Original)> Definitions = new();
        private readonly List<Component> Tombstones = new();
        private readonly CustomProperty Old;
        private readonly CustomProperty New;
        private readonly Type NewType;
        public IReadOnlyList<Component> AffectedRoots { get; }
        public IReadOnlyList<Component> AffectedComponents { get; }
        public IReadOnlyList<ValueChange> Values => Properties.Where(change => change.Value != null).Select(change => change.Value).ToList();
        internal IEnumerable<(Component Component, Property Property, ValueChange Value)> PreviewEntries =>
            Properties.Where(change => !change.Baseline).Select(change => (change.Component, change.Property, change.Value));

        public EditCustomPropertyPlan(IEnumerable<Component> roots, string declaringType, CustomProperty oldProperty, CustomProperty newProperty, Type newType)
        {
            Old = new CustomProperty(oldProperty);
            New = new CustomProperty(newProperty);
            NewType = newType;
            var rootList = roots.Distinct().ToList();
            var components = rootList.SelectMany(RemoveCustomPropertyPlan.Traverse).Distinct().ToList();
            bool Matches(Property property) => property.Name == Old.Name
                && (property.DeclaringTypeName ?? property.FromComponent?.FullName) == declaringType;
            bool typeChanged = Old.TypeName != New.TypeName;
            foreach (var component in components)
            {
                if (New.Name != Old.Name
                    && (component.Properties.Any(property => property.Name == New.Name && !Matches(property))
                        || component.CustomProperties.Any(property => property.Name == New.Name))
                    && (component.Properties.Any(Matches) || component.FullName == declaringType))
                    throw new InvalidOperationException($"'{New.Name}' already exists on {component.FullName}.");

                if (component.FullName == declaringType)
                {
                    foreach (var custom in component.CustomProperties.Where(property => property.Name == Old.Name))
                        Definitions.Add((component, custom, new CustomProperty(custom)));
                    if (New.Name != Old.Name && !component.RemovedCustomProperties.Contains(Old.Name))
                        Tombstones.Add(component);
                }

                void AddProperties(List<Property> source, bool baseline)
                {
                    foreach (var entry in source.Select((property, index) => (property, index)).Where(entry => Matches(entry.property)))
                    {
                        var property = entry.property;
                        var original = new Property(property);
                        var shouldFollowDefault = Equals(property.Value, Old.DefaultValue)
                            && (baseline || (!component.HasPropertyOverridesFor(Old.Name)
                                && !component.HasPersistedPropertyOverride(Old.Name)));
                        ValueChange value = null;
                        if (New.Access == CustomPropertyAccess.Public && typeChanged)
                        {
                            var converted = TryConvert(property.Value, newType, out var replacement);
                            var matchingValue = baseline && !converted && !shouldFollowDefault
                                ? Properties.FirstOrDefault(change => change.Component == component && !change.Baseline
                                    && change.Original.Name == property.Name && Equals(change.Original.Value, property.Value))?.Value
                                : null;
                            value = new ValueChange
                            {
                                Component = component,
                                Property = property,
                                IsBaseline = baseline,
                                Replacement = shouldFollowDefault ? New.DefaultValue : converted ? replacement : New.DefaultValue,
                                FollowValue = matchingValue
                            };
                        }
                        else if (New.Access == CustomPropertyAccess.Public && shouldFollowDefault)
                        {
                            value = new ValueChange { Component = component, Property = property, IsBaseline = baseline, Replacement = New.DefaultValue };
                        }
                        Properties.Add(new PropertyChange { Component = component, Property = property, Original = original, Baseline = baseline, Index = entry.index, Value = value });
                    }
                }
                AddProperties(component.Properties, false);
                AddProperties(component.OveriddenProperties, true);
            }

            var affected = Properties.Select(change => change.Component)
                .Concat(Definitions.Select(change => change.Component)).Concat(Tombstones).ToHashSet();
            AffectedComponents = affected.ToList();
            AffectedRoots = rootList.Where(root => RemoveCustomPropertyPlan.Traverse(root).Any(affected.Contains)).ToList();
        }

        private static bool TryConvert(object value, Type type, out object converted)
        {
            if (value == null)
            {
                converted = null;
                return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
            }
            if (type.IsInstanceOfType(value))
            {
                converted = value;
                return true;
            }
            try
            {
                converted = Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception exception) when (exception is InvalidCastException or FormatException or OverflowException)
            {
                converted = null;
                return false;
            }
        }

        public void Apply()
        {
            foreach (var entry in Definitions)
                CopyCustom(New, entry.Property);
            foreach (var component in Tombstones)
                component.RemovedCustomProperties.Add(Old.Name);
            foreach (var component in AffectedComponents)
                component.RenamePersistedPropertyOverride(Old.Name, New.Name);
            foreach (var change in Properties)
            {
                if (New.Access != CustomPropertyAccess.Public)
                {
                    (change.Baseline ? change.Component.OveriddenProperties : change.Component.Properties).Remove(change.Property);
                    continue;
                }
                change.Property.Name = New.Name;
                change.Property.Description = New.Description;
                change.Property.Group = New.Group;
                change.Property.Type = NewType;
                if (change.Value != null)
                    change.Property.Value = change.Value.FollowValue != null
                        ? change.Value.FollowValue.Replacement : change.Value.Replacement;
            }
        }

        public void Undo()
        {
            foreach (var entry in Definitions)
                CopyCustom(entry.Original, entry.Property);
            foreach (var component in Tombstones)
                component.RemovedCustomProperties.Remove(Old.Name);
            foreach (var component in AffectedComponents)
                component.RenamePersistedPropertyOverride(New.Name, Old.Name);
            foreach (var change in Properties)
            {
                var list = change.Baseline ? change.Component.OveriddenProperties : change.Component.Properties;
                if (!list.Contains(change.Property))
                    list.Insert(Math.Min(change.Index, list.Count), change.Property);
                change.Property.Name = change.Original.Name;
                change.Property.Description = change.Original.Description;
                change.Property.Group = change.Original.Group;
                change.Property.Type = change.Original.Type;
                change.Property.Value = change.Original.Value;
            }
        }

        private static void CopyCustom(CustomProperty source, CustomProperty target)
        {
            target.Name = source.Name;
            target.Description = source.Description;
            target.Group = source.Group;
            target.TypeName = source.TypeName;
            target.DefaultValue = source.DefaultValue;
            target.Access = source.Access;
        }
    }
}
