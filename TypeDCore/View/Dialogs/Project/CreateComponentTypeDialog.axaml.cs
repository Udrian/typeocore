using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Text.Json;
using TypeD.Helpers;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeD.ViewModel;
using TypeDCore.ViewModel.Dialogs.Project;
using TypeDCore.Models.Data.SettingContexts;

namespace TypeDCore.View.Dialogs.Project
{
    /// <summary>Dialog for creating an entity, scene, or drawable component.</summary>
    public partial class CreateComponentTypeDialog : Window
    {
        internal CreateComponentTypeViewModel ViewModel { get; }
        private readonly TypeD.Models.Data.Project project;
        private PropertyCreationSettingContext propertySettings;

        /// <summary>Creates a component creation dialog for the given project and initial root type.</summary>
        public CreateComponentTypeDialog(TypeD.Models.Data.Project project, string componentNamespace, Component initialRoot)
            : this(project, componentNamespace, initialRoot, Array.Empty<Component>(), new ComponentCreationSettingContext())
        {
        }

        /// <summary>Creates a component dialog with available module types and remembered base selections.</summary>
        public CreateComponentTypeDialog(TypeD.Models.Data.Project project, string componentNamespace, Component initialRoot,
            IEnumerable<Component> moduleTypes, IReadOnlyDictionary<string, string> rememberedBaseTypes)
        {
            InitializeComponent();
            this.project = project;
            DataContext = ViewModel = new CreateComponentTypeViewModel(this, project, componentNamespace, initialRoot,
                moduleTypes, rememberedBaseTypes);
        }

        /// <summary>Creates a component dialog with remembered creation fields.</summary>
        public CreateComponentTypeDialog(TypeD.Models.Data.Project project, string componentNamespace, Component initialRoot,
            IEnumerable<Component> moduleTypes, ComponentCreationSettingContext settings)
        {
            InitializeComponent();
            this.project = project;
            DataContext = ViewModel = new CreateComponentTypeViewModel(this, project, componentNamespace, initialRoot,
                moduleTypes, settings);
        }

        internal CreateComponentTypeDialog(TypeD.Models.Data.Project project, Component component,
            IEnumerable<Component> moduleTypes)
            : this(project, component.Namespace, component.BaseInheritedComponent, moduleTypes, new ComponentCreationSettingContext())
        {
            ViewModel.InitializeEditing(component);
            Title = "Edit Component";
            ComponentHeading.Text = "Edit Component";
            CreateButton.Content = "_Continue";
            RememberFields.IsVisible = false;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CommitBaseTypeText();
            if (ViewModel.Validate() && ViewModel.ValidatePropertyNames(ExtractPropertyNames()))
                Close(true);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close(false);
        private void OpenNamespace_Click(object sender, RoutedEventArgs e) => ViewModel.OpenNamespace();
        private async void OpenParent_Click(object sender, RoutedEventArgs e) => await ViewModel.OpenComponents();
        private void BaseType_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!((AutoCompleteBox)sender).IsDropDownOpen)
                ViewModel.CommitBaseTypeText();
        }
        private void BaseType_DropDownClosed(object sender, EventArgs e) => ViewModel.CommitBaseTypeText();
        private async void OpenIcon_Click(object sender, RoutedEventArgs e) => await ViewModel.OpenIcons();
        private void InheritIcon_Click(object sender, RoutedEventArgs e) => ViewModel.InheritIcon();

        private IEnumerable<string> ExtractPropertyNames()
        {
            var hook = new ExtractPropertiesHook(ViewModel.ParentComponent);
            ViewModelBase.ResourceModel.Get<IHookModel>().Shoot(hook);
            return hook.Properties.Select(property => property.Name);
        }

        private PropertyCreationSettingContext PropertySettings()
        {
            if (propertySettings != null)
                return propertySettings;
            var saved = ViewModelBase.ResourceModel.Get<ISettingModel>()
                .GetContext<PropertyCreationSettingContext>(SettingLevel.User);
            propertySettings = new PropertyCreationSettingContext();
            propertySettings.RememberFields.Value = saved.RememberFields.Value;
            propertySettings.TypeName.Value = saved.TypeName.Value;
            propertySettings.DefaultValueJson.Value = saved.DefaultValueJson.Value;
            propertySettings.Description.Value = saved.Description.Value;
            propertySettings.Group.Value = saved.Group.Value;
            propertySettings.Access.Value = saved.Access.Value;
            return propertySettings;
        }

        private async void AddProperty_Click(object sender, RoutedEventArgs e) => await EditProperty(null);
        private async void EditProperty_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CanEditDraftProperty)
                await EditProperty(ViewModel.SelectedProperty.Property);
        }

        private async Task EditProperty(CustomProperty original)
        {
            var resources = ViewModelBase.ResourceModel;
            var definitions = resources.Get<IPropertyModel>().List().ToList();
            var provider = resources.Get<IComponentProvider>();
            var components = provider.ListAll(project).Concat(provider.GetBaseTypeComponents()).Append(ViewModel.ParentComponent)
                .Append(new Component { CustomProperties = ViewModel.CustomProperties.ToList() });
            CustomProperty copy = null;
            if (original != null)
            {
                var type = definitions.Single(definition => definition.Type.FullName == original.TypeName
                    && definition.ValueType == definition.Type).Type;
                var value = JsonSerializer.SerializeToElement(original.DefaultValue, type, JSON.CreateOptions());
                copy = new CustomProperty(original) { DefaultValue = value.Deserialize(type, JSON.CreateOptions()) };
            }
            var dialog = new AddCustomPropertyDialog(definitions,
                ViewModel.TakenPropertyNames(original, ExtractPropertyNames()), components, copy, PropertySettings());
            if (await dialog.ShowDialog<bool>(this) != true)
                return;
            var property = dialog.ViewModel.Build();
            var selectedType = dialog.ViewModel.SelectedType.Type;
            if (original == null)
                ViewModel.AddProperty(property, selectedType, dialog.ViewModel.RememberFields);
            else
                ViewModel.ReplaceProperty(original, property, selectedType);

            if (ViewModel.PropertyPreference is { } preference && ReferenceEquals(preference.Property, property))
            {
                var settings = PropertySettings();
                settings.RememberFields.Value = preference.RememberFields;
                if (preference.RememberFields)
                {
                    settings.TypeName.Value = selectedType.FullName;
                    settings.DefaultValueJson.Value = JsonSerializer.Serialize(property.DefaultValue, selectedType, JSON.CreateOptions());
                    settings.Description.Value = property.Description;
                    settings.Group.Value = property.Group;
                    settings.Access.Value = property.Access;
                }
            }
        }

        private void RemoveProperty_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CanEditDraftProperty)
                ViewModel.RemoveProperty(ViewModel.SelectedProperty.Property);
        }
    }
}
