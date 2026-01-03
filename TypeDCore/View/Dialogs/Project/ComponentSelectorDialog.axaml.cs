using Avalonia.Controls;
using Avalonia.Interactivity;
using TypeD.Models.Data;
using TypeDCore.ViewModel.Dialogs.Project;

namespace TypeDCore.View.Dialogs.Project
{
    /// <summary>
    /// Interaction logic for ComponentSelectorDialog.xaml
    /// </summary>
    public partial class ComponentSelectorDialog : Window
    {
        // ViewModel
        internal ComponentSelectorViewModel ViewModel { get; set; }

        // Constructors
        public ComponentSelectorDialog(TypeD.Models.Data.Project project)
        {
            InitializeComponent();
            DataContext = ViewModel = new ComponentSelectorViewModel(this, project);
        }

        // Event Handlers
        private void lbComponents_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (lbComponents.SelectedItem is not Component component) return;
            ViewModel.SelectedComponent = component;
            Close(true);
        }

        private void tbFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.NameFilter.Filters = tbFilter.Text;
            ViewModel.UpdateFilter();
        }
    }
}
