using System.Windows;
using System.Windows.Controls;
using TypeD.Models.Data;
using TypeDCore.ViewModel.Panels;

namespace TypeDCore.View.Panels
{
    /// <summary>
    /// Interaction logic for ComponentPanel.xaml
    /// </summary>
    public partial class ComponentPanel : UserControl
    {
        // ViewModel
        ComponentViewModel ViewModel { get; set; }
        
        // Constructors
        public ComponentPanel(Project project)
        {
            DataContext = ViewModel = new ComponentViewModel(this, project);
            InitializeComponent();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ViewModel.ContextMenuOpened(sender as ContextMenu, ComponentsTree.SelectedItem as ComponentViewModel.Node);
        }
    }
}
