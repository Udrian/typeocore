using System.Windows;
using TypeDCore.ViewModel.Dialogs.Project;

namespace TypeDCore.View.Dialogs.Project
{
    /// <summary>
    /// Interaction logic for CreateSceneDialog.xaml
    /// </summary>
    public partial class CreateSceneTypeDialog : Window
    {
        // ViewModel
        internal CreateComponentTypeBaseViewModel ViewModel { get; set; }

        // Constructors
        public CreateSceneTypeDialog(TypeD.Models.Data.Project project, string @namespace)
        {
            InitializeComponent();
            ViewModel = new CreateComponentTypeBaseViewModel(project, @namespace, typeof(TypeOEngine.Typedeaf.Core.Scene).FullName);
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
