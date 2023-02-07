using System.Windows.Controls;
using TypeD.Models.Data;
using TypeD.View.Viewer;
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

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.Source is TabControl)
            {
                ViewerViewModel.TabSelectionChanged((e.Source as TabControl).SelectedContent as IViewer);
            }
        }
    }
}
