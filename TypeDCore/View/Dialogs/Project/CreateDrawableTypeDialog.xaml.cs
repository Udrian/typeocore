using System.Windows;
using TypeDCore.ViewModel.Dialogs.Project;

namespace TypeDCore.View.Dialogs.Project
{
    /// <summary>
    /// Interaction logic for CreateDrawable2dDialog.xaml
    /// </summary>
    public partial class CreateDrawableTypeDialog : Window
    {
        // ViewModel
        internal CreateComponentTypeBaseViewModel ViewModel { get; set; }

        // Constructors
        public CreateDrawableTypeDialog(TypeD.Models.Data.Project project, string @namespace)
        {
            InitializeComponent();
            ViewModel = new CreateComponentTypeBaseViewModel(project, @namespace, typeof(TypeOEngine.Typedeaf.Core.Entities.Drawables.Drawable).FullName);
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
