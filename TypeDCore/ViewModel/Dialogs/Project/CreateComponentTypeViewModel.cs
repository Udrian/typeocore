using Avalonia.Controls;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis.CSharp;
using TypeD.Models.Data;
using TypeD.View.TreeNodes;
using TypeDCore.Components;
using TypeDCore.Models.Data.SettingContexts;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;
using TypeOEngine.Typedeaf.Core.Interfaces;

namespace TypeDCore.ViewModel.Dialogs.Project
{
    internal class CreateComponentTypeViewModel : CreateEntityTypeViewModel
    {
        internal class PropertyRow
        {
            public CustomProperty Property { get; init; }
            public Component DeclaringComponent { get; init; }
            public bool IsInherited => DeclaringComponent != null;
            public string InheritedFrom => IsInherited ? $" (inherited from {DeclaringComponent.FullName})" : "";
            public string Details => $"{Property.Access} {Property.TypeName} - {Property.Group}";
        }

        private readonly List<CustomProperty> customProperties = new();
        private readonly Dictionary<CustomProperty, CustomProperty> originalProperties = new();
        public IReadOnlyList<CustomProperty> CustomProperties => customProperties;
        public ObservableCollection<PropertyRow> PropertyRows { get; } = new();
        private PropertyRow selectedProperty;
        public PropertyRow SelectedProperty
        {
            get => selectedProperty;
            set
            {
                selectedProperty = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanEditProperty));
                OnPropertyChanged(nameof(CanEditDraftProperty));
            }
        }
        public bool CanEditProperty => SelectedProperty != null && !SelectedProperty.IsInherited;
        public bool CanEditDraftProperty => CanEditProperty;
        public (CustomProperty Property, Type Type, bool RememberFields)? PropertyPreference { get; private set; }

        private Component selectedRoot;
        private string automaticNamespace;
        private bool settingAutomaticNamespace;
        private readonly Dictionary<string, Component> parents = new();
        private readonly List<Component> availableBaseTypes;
        private string baseTypeText;
        private bool baseTypeConfirmed;
        private Component editingComponent;

        public IReadOnlyList<Component> RootTypes { get; } = new[]
        {
            CoreComponent.EntityComponent(),
            CoreComponent.SceneComponent(),
            CoreComponent.DrawableComponent()
        };

        public Component SelectedRoot
        {
            get => selectedRoot;
            set
            {
                if (value == null || selectedRoot == value)
                    return;

                parents[selectedRoot.FullName] = ParentComponent;
                selectedRoot = value;
                ComponentBaseType = value.FullName;
                OnPropertyChanged(nameof(BaseTypeSuggestions));
                if (automaticNamespace != null && ComponentNamespace == automaticNamespace)
                {
                    automaticNamespace = QualifyNamespace(DefaultNamespace(value));
                    settingAutomaticNamespace = true;
                    ComponentNamespace = automaticNamespace;
                    settingAutomaticNamespace = false;
                }
                else
                {
                    automaticNamespace = null;
                }

                OnParentComponentSet(parents.TryGetValue(value.FullName, out var previousParent) ? previousParent
                    : availableBaseTypes.FirstOrDefault(component => component.FullName == value.FullName) ?? value);
                OnPropertyChanged(nameof(SelectedRoot));
                OnPropertyChanged(nameof(ParentComponentFullName));
                OnPropertyChanged(nameof(IsEntity));
            }
        }

        public bool IsEntity => selectedRoot?.FullName == typeof(TypeOEngine.Typedeaf.Core.Entities.Entity).FullName;
        public bool RememberFields { get; set; }
        public string ValidationError { get; private set; }
        public string FullNamespace => QualifyNamespace(ComponentNamespace);
        public string ResultName => string.IsNullOrEmpty(ComponentName) ? FullNamespace : $"{FullNamespace}.{ComponentName}";
        public IReadOnlyList<string> NamespaceSuggestions { get; }
        public IReadOnlyList<string> BaseTypeSuggestions => availableBaseTypes
            .Where(component => component.RootBaseComponent.FullName == ComponentBaseType)
            .OrderBy(component => component.ClassName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(component => component.FullName, StringComparer.Ordinal)
            .Select(component => component.FullName).ToList();
        public string BaseTypeText
        {
            get => baseTypeText;
            set
            {
                if (baseTypeText == value)
                    return;
                baseTypeText = value;
                baseTypeConfirmed = false;
                OnPropertyChanged(nameof(BaseTypeText));
                Validate();
            }
        }

        public CreateComponentTypeViewModel(Control element, TypeD.Models.Data.Project project, string componentNamespace, Component initialRoot)
            : this(element, project, componentNamespace, initialRoot, Array.Empty<Component>(), new ComponentCreationSettingContext())
        {
        }

        public CreateComponentTypeViewModel(Control element, TypeD.Models.Data.Project project, string componentNamespace,
            Component initialRoot, IEnumerable<Component> moduleTypes, IReadOnlyDictionary<string, string> rememberedBaseTypes)
            : this(element, project, componentNamespace, initialRoot, moduleTypes, new ComponentCreationSettingContext
            {
                RememberFields = new Setting<bool>(true),
                LastBaseTypes = new Setting<Dictionary<string, string>>(new Dictionary<string, string>(rememberedBaseTypes))
            })
        {
        }

        public CreateComponentTypeViewModel(Control element, TypeD.Models.Data.Project project, string componentNamespace,
            Component initialRoot, IEnumerable<Component> moduleTypes, ComponentCreationSettingContext settings)
            : base(element, project, componentNamespace ?? DefaultNamespace(initialRoot?.RootBaseComponent ?? CoreComponent.EntityComponent()),
                initialRoot ?? CoreComponent.EntityComponent())
        {
            RememberFields = settings.RememberFields.Value;
            var rememberedRoot = RememberFields && initialRoot == null
                ? RootTypes.FirstOrDefault(root => root.FullName == settings.RootType.Value) : null;
            ComponentBaseType = (rememberedRoot ?? initialRoot ?? CoreComponent.EntityComponent()).RootBaseComponent.FullName;
            selectedRoot = RootTypes.First(root => root.FullName == ComponentBaseType);
            availableBaseTypes = moduleTypes.Concat(GetComponents(Project.ComponentTree.Nodes)).Concat(RootTypes)
                .Concat(initialRoot == null ? Array.Empty<Component>() : new[] { initialRoot })
                .GroupBy(component => component.FullName, StringComparer.Ordinal)
                .Select(group => group.First()).ToList();
            foreach (var root in RootTypes)
            {
                if (RememberFields && settings.LastBaseTypes.Value.TryGetValue(root.FullName, out var remembered))
                {
                    var component = availableBaseTypes.FirstOrDefault(candidate =>
                        candidate.FullName == remembered && candidate.RootBaseComponent.FullName == root.FullName);
                    if (component != null)
                        parents[root.FullName] = component;
                }
            }
            if (RememberFields)
            {
                if (componentNamespace == null && !string.IsNullOrEmpty(settings.Namespace.Value))
                    ComponentNamespace = settings.Namespace.Value;
                else if (componentNamespace == null)
                    ComponentNamespace = DefaultNamespace(selectedRoot);
                IconName = settings.Icon.Value;
                ComponentUpdatable = settings.Updatable.Value;
                ComponentDrawable = settings.Drawable.Value;
            }
            ComponentNamespace = QualifyNamespace(ComponentNamespace);
            var explicitBase = initialRoot != null && initialRoot.FullName != ComponentBaseType;
            OnParentComponentSet(explicitBase ? initialRoot
                : parents.TryGetValue(ComponentBaseType, out var lastParent) ? lastParent
                : availableBaseTypes.First(component => component.FullName == selectedRoot.FullName));
            if (componentNamespace == null && (!RememberFields || string.IsNullOrEmpty(settings.Namespace.Value)))
                automaticNamespace = ComponentNamespace;
            NamespaceSuggestions = GetComponents(Project.ComponentTree.Nodes)
                .Select(component => component.Namespace)
                .Append(Project.ProjectName)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(ComponentName) || args.PropertyName == nameof(ComponentNamespace))
                {
                    if (args.PropertyName == nameof(ComponentNamespace) && !settingAutomaticNamespace)
                        automaticNamespace = null;
                    OnPropertyChanged(nameof(FullNamespace));
                    OnPropertyChanged(nameof(ResultName));
                    Validate();
                }
                else if (args.PropertyName == nameof(NamespaceSelectionError) || args.PropertyName == nameof(ParentComponent))
                {
                    Validate();
                }
            };
        }

        public override bool Validate()
        {
            var namespaceParts = (ComponentNamespace ?? "").Split('.');
            if (NamespaceSelectionError != null)
                ValidationError = NamespaceSelectionError;
            else if (editingComponent != null && IsDescendantOrSelf(ParentComponent, editingComponent.FullName))
                ValidationError = "A component cannot inherit from itself or one of its derived types.";
            else if (ParentComponent?.RootBaseComponent.FullName != ComponentBaseType ||
                !baseTypeConfirmed || BaseTypeText != ParentComponent.ClassName)
                ValidationError = "Select an existing base type for the chosen component type.";
            else if (!IsIdentifier(ComponentName))
                ValidationError = "Enter a valid C# component name that can be used as a file name.";
            else if (string.IsNullOrEmpty(ComponentNamespace) || namespaceParts.Any(part => !IsIdentifier(part)))
                ValidationError = "Enter a valid C# namespace with valid folder names.";
            else if (!string.Equals(editingComponent?.FullName, ResultName, OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) &&
                (GetComponents(Project.ComponentTree.Nodes).Any(component =>
                    string.Equals(component.FullName, ResultName, OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) ||
                File.Exists(Path.Combine(Project.ProjectTypeOPath, "components",
                    FullNamespace.Replace('.', Path.DirectorySeparatorChar), ComponentName + ".component"))))
                ValidationError = $"A component named '{ResultName}' already exists.";
            else if (FindPropertyConflict(Enumerable.Empty<string>()) is string conflict)
                ValidationError = $"A property named '{conflict}' conflicts with the component name or its base type.";
            else
                ValidationError = null;
            OnPropertyChanged(nameof(ValidationError));
            return ValidationError == null;
        }

        public override void OnParentComponentSet(Component parentComponent)
        {
            if (availableBaseTypes != null &&
                (editingComponent == null || !IsDescendantOrSelf(parentComponent, editingComponent.FullName)) &&
                !availableBaseTypes.Any(component => component.FullName == parentComponent.FullName))
            {
                availableBaseTypes.Add(parentComponent);
                OnPropertyChanged(nameof(BaseTypeSuggestions));
            }
            baseTypeText = parentComponent.ClassName;
            baseTypeConfirmed = true;
            base.OnParentComponentSet(parentComponent);
            RefreshPropertyRows();
            OnPropertyChanged(nameof(BaseTypeText));
        }

        public HashSet<string> TakenPropertyNames(CustomProperty except, IEnumerable<string> additionalNames)
        {
            var names = new HashSet<string>(additionalNames, StringComparer.Ordinal);
            if (!string.IsNullOrEmpty(ComponentName))
                names.Add(ComponentName);
            for (var component = ParentComponent; component != null; component = component.BaseInheritedComponent)
            {
                names.UnionWith(component.Properties.Select(property => property.Name));
                names.UnionWith(component.CustomProperties.Select(property => property.Name));
            }
            names.UnionWith(customProperties.Where(property => !ReferenceEquals(property, except)).Select(property => property.Name));
            return names;
        }

        public bool ValidatePropertyNames(IEnumerable<string> additionalNames)
        {
            var conflict = FindPropertyConflict(additionalNames);
            if (conflict == null)
                return true;
            ValidationError = $"A property named '{conflict}' conflicts with the component name or its base type.";
            OnPropertyChanged(nameof(ValidationError));
            return false;
        }

        private string FindPropertyConflict(IEnumerable<string> additionalNames)
        {
            foreach (var property in customProperties)
            {
                if (TakenPropertyNames(property, additionalNames).Contains(property.Name))
                    return property.Name;
            }
            return null;
        }

        public void AddProperty(CustomProperty property, Type type, bool rememberFields)
        {
            customProperties.Add(property);
            PropertyPreference = (property, type, rememberFields);
            RefreshPropertyRows();
            Validate();
        }

        public void ReplaceProperty(CustomProperty original, CustomProperty replacement, Type type)
        {
            var index = customProperties.IndexOf(original);
            if (index < 0)
                throw new InvalidOperationException("The selected property is not a draft property.");
            customProperties[index] = replacement;
            if (originalProperties.Remove(original, out var source))
                originalProperties.Add(replacement, source);
            if (PropertyPreference is { } preference && ReferenceEquals(preference.Property, original))
                PropertyPreference = (replacement, type, preference.RememberFields);
            RefreshPropertyRows();
            Validate();
        }

        public void RemoveProperty(CustomProperty property)
        {
            if (!customProperties.Remove(property))
                throw new InvalidOperationException("The selected property is not a draft property.");
            originalProperties.Remove(property);
            if (PropertyPreference is { } preference && ReferenceEquals(preference.Property, property))
                PropertyPreference = null;
            RefreshPropertyRows();
            Validate();
        }

        public CustomProperty OriginalProperty(CustomProperty draft) =>
            originalProperties.TryGetValue(draft, out var original) ? original : null;

        public void InitializeEditing(Component component)
        {
            ArgumentNullException.ThrowIfNull(component);
            if (editingComponent != null)
                throw new InvalidOperationException("The edit draft has already been initialized.");
            if (component.BaseInheritedComponent == null)
                throw new InvalidOperationException("The component has no editable base type.");
            editingComponent = component;
            automaticNamespace = null;
            availableBaseTypes.RemoveAll(candidate => IsDescendantOrSelf(candidate, component.FullName));
            ComponentName = component.ClassName;
            ComponentNamespace = component.Namespace;
            IconName = component.Icon;
            ComponentUpdatable = component.HasInterface(typeof(IUpdatable));
            ComponentDrawable = component.HasInterface(typeof(IDrawable));
            ComponentUpdatableSet = null;
            ComponentDrawableSet = null;
            OnParentComponentSet(component.BaseInheritedComponent);
            OnPropertyChanged(nameof(ComponentUpdatable));
            OnPropertyChanged(nameof(ComponentDrawable));
            foreach (var original in component.CustomProperties)
            {
                var draft = new CustomProperty(original);
                customProperties.Add(draft);
                originalProperties.Add(draft, original);
            }
            RefreshPropertyRows();
            OnPropertyChanged(nameof(BaseTypeSuggestions));
            OnPropertyChanged(nameof(IconPreview));
            OnPropertyChanged(nameof(IconText));
            Validate();
        }

        private static bool IsDescendantOrSelf(Component candidate, string fullName)
        {
            var visited = new HashSet<Component>();
            for (var current = candidate; current != null && visited.Add(current); current = current.BaseInheritedComponent)
                if (current.FullName == fullName)
                    return true;
            return false;
        }

        private void RefreshPropertyRows()
        {
            PropertyRows.Clear();
            SelectedProperty = null;
            for (var component = ParentComponent; component != null; component = component.BaseInheritedComponent)
                foreach (var property in component.CustomProperties.Where(property => property.Access != CustomPropertyAccess.Private))
                    PropertyRows.Add(new PropertyRow { Property = property, DeclaringComponent = component });
            foreach (var property in customProperties)
                PropertyRows.Add(new PropertyRow { Property = property });
        }

        public void CommitBaseTypeText()
        {
            if (baseTypeConfirmed && BaseTypeText == ParentComponent.ClassName)
                return;
            var matches = availableBaseTypes.Where(component =>
                component.RootBaseComponent.FullName == ComponentBaseType &&
                (string.Equals(component.FullName, BaseTypeText, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(component.ClassName, BaseTypeText, StringComparison.OrdinalIgnoreCase))).ToList();
            if (matches.Count == 1)
                OnParentComponentSet(matches[0]);
            else
                Validate();
        }

        private string QualifyNamespace(string value)
        {
            if (string.IsNullOrEmpty(value))
                return Project.ProjectName;
            return value == Project.ProjectName || value.StartsWith(Project.ProjectName + ".", StringComparison.Ordinal)
                ? value : Project.ProjectName + "." + value;
        }

        private static bool IsIdentifier(string value)
        {
            if (string.IsNullOrEmpty(value) || value.StartsWith("@", StringComparison.Ordinal) ||
                !SyntaxFacts.IsValidIdentifier(value) || SyntaxFacts.GetKeywordKind(value) != SyntaxKind.None)
                return false;
            if (!OperatingSystem.IsWindows())
                return true;
            var upper = value.ToUpperInvariant();
            return upper != "CON" && upper != "PRN" && upper != "AUX" && upper != "NUL" &&
                !(upper.Length == 4 && (upper.StartsWith("COM", StringComparison.Ordinal) || upper.StartsWith("LPT", StringComparison.Ordinal))
                    && upper[3] >= '1' && upper[3] <= '9');
        }

        private static IEnumerable<Component> GetComponents(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Item is Component component)
                    yield return component;
                foreach (var child in GetComponents(node.Nodes))
                    yield return child;
            }
        }

        private static string DefaultNamespace(Component root)
        {
            if (root.FullName == typeof(TypeOEngine.Typedeaf.Core.Entities.Entity).FullName)
                return "Entities";
            if (root.FullName == typeof(TypeOEngine.Typedeaf.Core.Scene).FullName)
                return "Scenes";
            return "Drawables";
        }
    }
}
