using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using TypeD.Models.Data;
using TypeD.ViewModel;
using TypeDCore.Commands;

namespace TypeDCore.View.Dialogs.Project
{
    /// <summary>Confirms property edits or removal, showing affected components and code.</summary>
    public partial class PropertyChangePreviewDialog : Window
    {
        private sealed class PreviewEntry
        {
            public Component Component { get; init; }
            public Property Property { get; init; }
            public EditCustomPropertyPlan.ValueChange Value { get; init; }
        }

        private sealed class PreviewGroup
        {
            public string Name { get; init; }
            public List<PreviewRow> Rows { get; } = new();
        }

        private sealed class PreviewRow
        {
            private readonly EditCustomPropertyPlan.ValueChange Change;
            public string NodePath { get; }
            public string OldValue { get; }
            public string NewValue { get; }
            public bool HasEditor => Node != null;
            public bool HasTextValue => Node == null;
            public PropertyNode Node { get; }
            public IDataTemplate Template { get; }

            public PreviewRow(string path, PreviewEntry entry, bool removed, IPropertyDefinition definition, bool editValue)
            {
                Change = entry.Value;
                NodePath = path;
                OldValue = FormatValue(entry.Property.Value);
                NewValue = removed ? "(removed)" : FormatValue(entry.Value != null ? entry.Value.Replacement : entry.Property.Value);
                if (!editValue || entry.Value == null)
                    return;
                if (definition == null)
                    throw new InvalidOperationException("The replacement value editor is unavailable.");

                var property = new Property { Name = entry.Property.Name, Type = definition.Type, Value = entry.Value.Replacement };
                void ValueChanged(PropertyNode node, object value)
                {
                    node.Property.Value = value;
                    Change.Replacement = value;
                    node.RefreshValue();
                }
                Node = definition.CreateNode(entry.Component, property, ValueChanged)
                    ?? new PropertyNode(entry.Component, property, ValueChanged);
                Template = definition.CreateTemplate();
            }

            private static string FormatValue(object value) => value?.ToString() ?? "(null)";
        }

        private readonly List<PreviewRow> Rows = new();

        internal PropertyChangePreviewDialog(EditCustomPropertyPlan plan, IPropertyDefinition definition,
            IEnumerable<string> files, bool typeChanged, bool accessChanged, bool removed)
        {
            InitializeComponent();
            message.Text = "Apply these property changes? Save affected files in other editors first. " +
                "Source changes are written immediately; component changes are saved with the project." +
                " References outside the loaded C# project may need manual updates." +
                (accessChanged ? " Non-public properties are removed from the property panel and their saved overrides are removed." : "");
            var entries = plan.PreviewEntries.Select(entry => new PreviewEntry
            {
                Component = entry.Component, Property = entry.Property, Value = entry.Value
            });
            Populate(plan.AffectedRoots, entries, files, removed, definition, typeChanged);
        }

        internal PropertyChangePreviewDialog(string propertyName, RemoveCustomPropertyPlan plan, IEnumerable<string> files)
        {
            InitializeComponent();
            Title = "Remove property";
            message.Text = $"Remove '{propertyName}'? Values on affected components will be removed. " +
                "The code files listed below may need manual changes.";
            confirmButton.Content = "_Remove";
            var entries = plan.PreviewEntries.Select(entry => new PreviewEntry
            {
                Component = entry.Component, Property = entry.Property
            });
            Populate(plan.AffectedRoots, entries, files, true, null, false);
        }

        private void Populate(IEnumerable<Component> roots, IEnumerable<PreviewEntry> entries, IEnumerable<string> files,
            bool removed, IPropertyDefinition definition, bool editValues)
        {
            var entryList = entries.ToList();
            var lookup = entryList.ToLookup(entry => entry.Component);
            var groups = roots.Select(root => (Root: root, Group: new PreviewGroup { Name = root.FullName })).ToList();
            var visited = new HashSet<Component>();

            void Visit(Component component, PreviewGroup group, List<string> path, bool inherited)
            {
                if (!visited.Add(component))
                    return;
                var nodePath = string.Join(Environment.NewLine, path.Select((name, index) => new string(' ', index * 2) + name));
                foreach (var entry in lookup[component])
                {
                    if (path.Count == 1
                        && (entry.Property.DeclaringTypeName ?? entry.Property.FromComponent?.FullName) == component.FullName)
                        continue;
                    if (inherited && !component.HasPropertyOverridesFor(entry.Property.Name))
                        continue;
                    var row = new PreviewRow(nodePath, entry, removed, definition, editValues);
                    group.Rows.Add(row);
                    Rows.Add(row);
                }
                foreach (var child in component.Children)
                    Visit(child, group, path.Append(DisplayName(child)).ToList(),
                        path.Count > 1 && component.DefinitionChildren.Contains(child));
            }

            foreach (var item in groups)
                Visit(item.Root, item.Group, new List<string> { DisplayName(item.Root) }, false);
            var visibleGroups = groups.Select(item => item.Group).Where(group => group.Rows.Count > 0).ToList();
            componentGroups.ItemsSource = visibleGroups;
            noComponentReferences.IsVisible = visibleGroups.Count == 0;
            var sourceFiles = files.ToList();
            sourceHeading.IsVisible = sourceFiles.Count > 0;
            sources.ItemsSource = sourceFiles;
        }

        private static string DisplayName(Component component) =>
            !string.IsNullOrWhiteSpace(component.Name) ? component.Name : component.FullName;

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close(false);

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (Rows.Any(row => row.Node?.HasValidationError == true))
            {
                error.Text = "Correct the invalid replacement values.";
                return;
            }
            Close(true);
        }
    }
}
