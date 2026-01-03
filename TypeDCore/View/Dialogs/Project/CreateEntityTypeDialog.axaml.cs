using Avalonia.Controls;
using Avalonia.Interactivity;
using TypeDCore.Components;
using TypeDCore.ViewModel.Dialogs.Project;

namespace TypeDCore.View.Dialogs.Project
{
    /// <summary>
    /// Interaction logic for CreateEntityTypeDialog.xaml
    /// </summary>
    public partial class CreateEntityTypeDialog : Window
    {
        // ViewModel
        internal CreateEntityTypeViewModel ViewModel { get; set; }

        // Constructors
        public CreateEntityTypeDialog(TypeD.Models.Data.Project project, string @namespace)
        {
            InitializeComponent();
            ViewModel = new CreateEntityTypeViewModel(this, project, @namespace, CoreComponent.EntityComponent());
            this.DataContext = ViewModel;
        }

        // Event Handlers
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.Validate())
                return;
                
            Close(true);
        }

        private void btnOpenNamespace_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenNamespace();
        }

        private void btnOpenInherit_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenComponents();
        }
    }
}
