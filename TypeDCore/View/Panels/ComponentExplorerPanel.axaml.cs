using Avalonia.Controls;
using Avalonia.Interactivity;
using TypeD.Models.Data;
using TypeDCore.ViewModel.Panels;

namespace TypeDCore.View.Panels
{
    /// <summary>
    /// Interaction logic for ComponentPanel.xaml
    /// </summary>
    public partial class ComponentExplorerPanel : UserControl
    {
        // ViewModel
        ComponentExplorerViewModel ViewModel { get; set; }
        
        // Constructors
        public ComponentExplorerPanel(Project project)
        {
            DataContext = ViewModel = new ComponentExplorerViewModel(this, project);
            InitializeComponent();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ViewModel.ContextMenuOpened(sender as ContextMenu, ComponentsTree.SelectedItem as ComponentExplorerViewModel.Node);
        }
    }
}
