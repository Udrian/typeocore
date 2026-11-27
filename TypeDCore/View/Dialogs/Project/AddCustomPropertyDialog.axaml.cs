using Avalonia.Controls;
using Avalonia.Interactivity;
using TypeD.Models.Data;
using TypeDCore.ViewModel.Dialogs.Project;
using TypeDCore.Models.Data.SettingContexts;

namespace TypeDCore.View.Dialogs.Project
{
    /// <summary>
    /// Interaction logic for AddCustomPropertyDialog.xaml
    /// </summary>
    public partial class AddCustomPropertyDialog : Window
    {
        // ViewModel
        internal AddCustomPropertyViewModel ViewModel { get; set; }

        // Constructors
        /// <summary>Creates a dialog for adding a custom property to a component.</summary>
        /// <param name="definitions">The property definitions the user can pick a value type from.</param>
        /// <param name="takenNames">Property names that are already in use on the component.</param>
        public AddCustomPropertyDialog(IEnumerable<IPropertyDefinition> definitions, IEnumerable<string> takenNames)
            : this(definitions, takenNames, Enumerable.Empty<Component>())
        {
        }

        /// <summary>Creates a dialog for adding a custom property with suggested groups.</summary>
        /// <param name="definitions">The property definitions the user can pick a value type from.</param>
        /// <param name="takenNames">Property names that are already in use on the component.</param>
        /// <param name="components">Components whose property groups can be suggested.</param>
        public AddCustomPropertyDialog(IEnumerable<IPropertyDefinition> definitions, IEnumerable<string> takenNames, IEnumerable<Component> components)
            : this(definitions, takenNames, components, null)
        {
        }

        /// <summary>Creates a dialog for adding or editing a custom property.</summary>
        /// <param name="definitions">The available property definitions.</param>
        /// <param name="takenNames">Other property names already in use.</param>
        /// <param name="components">Components whose property groups can be suggested.</param>
        /// <param name="existing">The property being edited, or null when adding.</param>
        public AddCustomPropertyDialog(IEnumerable<IPropertyDefinition> definitions, IEnumerable<string> takenNames, IEnumerable<Component> components, CustomProperty existing)
            : this(definitions, takenNames, components, existing, new PropertyCreationSettingContext())
        {
        }

        /// <summary>Creates a property dialog with remembered creation fields.</summary>
        public AddCustomPropertyDialog(IEnumerable<IPropertyDefinition> definitions, IEnumerable<string> takenNames, IEnumerable<Component> components,
            CustomProperty existing, PropertyCreationSettingContext settings)
        {
            InitializeComponent();
            DataContext = ViewModel = new AddCustomPropertyViewModel(this, definitions, takenNames, components, existing, settings);
            if (existing != null)
            {
                Title = "Edit Property";
                dialogHeading.Text = "Edit Property";
                submitButton.Content = "_Save";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Validate())
                Close(true);
            else
                validationError.BringIntoView();
        }
    }
}
