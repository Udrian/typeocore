using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
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

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, PointerPressedEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch<TreeViewItem>(sender as AvaloniaObject);
            
            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = true;
                e.Handled = true;
            }
        }

        private void TreeViewItem_MouseDoubleClickEvent(object sender, PointerPressedEventArgs e)
        {
            if(((TreeViewItem)sender).Header == TreeView.SelectedItem)
                ViewModel.DoubleClickItem(TreeView.SelectedItem as ComponentBrowserViewModel.Node);
        }

        static T VisualUpwardSearch<T>(AvaloniaObject source) where T : AvaloniaObject
        {
            AvaloniaObject returnVal = source;

            //while (returnVal != null && !(returnVal is T))
            //{
            //    AvaloniaObject tempReturnVal = null;
            //    if (returnVal is Visual || returnVal is Visual3D)
            //    {
            //        tempReturnVal = VisualTreeHelper.GetParent(returnVal);
            //    }
            //    if (tempReturnVal == null)
            //    {
            //        returnVal = LogicalTreeHelper.GetParent(returnVal);
            //    }
            //    else returnVal = tempReturnVal;
            //}

            return returnVal as T;
        }
    }
}
