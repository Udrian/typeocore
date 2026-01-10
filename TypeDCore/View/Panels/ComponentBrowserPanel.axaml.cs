using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System.Linq;
using TypeD.Models.Data;
using TypeDCore.ViewModel.Panels;

namespace TypeDCore.View.Panels
{
    /// <summary>
    /// Interaction logic for ComponentBrowserPanel.xaml
    /// </summary>
    public partial class ComponentBrowserPanel : UserControl
    {
        // ViewModel
        ComponentBrowserViewModel ViewModel { get; set; }

        // Constructors
        public ComponentBrowserPanel(Project project)
        {
            InitializeComponent();

            ViewModel = new ComponentBrowserViewModel(this, project, TreeView);
            DataContext = ViewModel;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ViewModel.ContextMenuOpened(sender as ContextMenu, TreeView.SelectedItem as ComponentBrowserViewModel.Node);
        }

        private void TreeViewItem_MouseDoubleClickEvent(object sender, TappedEventArgs e)
        {
            var item = ((Visual)e.Source!).GetSelfAndVisualAncestors()
            .OfType<TreeViewItem>()
            .FirstOrDefault();

            if (item is not null)
            {
               if (item.DataContext is ComponentBrowserViewModel.Node node)
               {
                    ViewModel.DoubleClickItem(node);
               }
            }
        }
    }
}
