using System.Windows;
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
            ViewModel = new CreateEntityTypeViewModel(project, @namespace, typeof(TypeOEngine.Typedeaf.Core.Entities.Entity).FullName);
            this.DataContext = ViewModel;
        }

        // Event Handlers
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.Validate())
                return;

            DialogResult = true;
            Close();
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
