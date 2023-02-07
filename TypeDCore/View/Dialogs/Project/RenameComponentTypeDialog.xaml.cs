using System.Windows;
using TypeD.Models.Data;
using TypeDCore.ViewModel.Dialogs.Project;

namespace TypeDCore.View.Dialogs.Project
{
    /// <summary>
    /// Interaction logic for RenameComponentTypeDialog.xaml
    /// </summary>
    public partial class RenameComponentTypeDialog : Window
    {
        // ViewModel
        internal RenameComponentTypeViewModel ViewModel { get; set; }

        // Constructors
        public RenameComponentTypeDialog(Component component)
        {
            InitializeComponent();
            DataContext = ViewModel = new RenameComponentTypeViewModel(this, component);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = ViewModel.Name != ViewModel.OldName;
            Close();
        }
    }
}
