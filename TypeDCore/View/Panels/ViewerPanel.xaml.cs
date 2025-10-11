using System.Windows.Controls;
using TypeD.Models.Data;
using TypeDCore.ViewModel.Panels;

namespace TypeDCore.View.Panels
{
    /// <summary>
    /// Interaction logic for ViewerPanel.xaml
    /// </summary>
    public partial class ViewerPanel : UserControl
    {
        ViewerViewModel ViewerViewModel { get; set; }

        public ViewerPanel(Project project)
        {
            InitializeComponent();

            DataContext = ViewerViewModel = new ViewerViewModel(project, this);
        }

        private void ViewerPanelUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewerViewModel.Unload();
        }
    }
}
