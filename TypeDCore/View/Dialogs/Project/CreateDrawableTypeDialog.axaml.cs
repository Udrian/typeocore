using Avalonia.Controls;
using Avalonia.Interactivity;
using TypeDCore.Components;
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
            ViewModel = new CreateComponentTypeBaseViewModel(this, project, @namespace, CoreComponent.DrawableComponent());
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
