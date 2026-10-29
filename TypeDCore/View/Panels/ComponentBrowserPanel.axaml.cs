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

        private void ContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
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
                    ViewModel.DoubleClickItem(node);
            }
        }

        private void TreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.SelectionChanged(TreeView.SelectedItem as ComponentBrowserViewModel.Node);
        }

        private void TreeView_GotFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.BrowserFocused();
        }

        private void TreeView_KeyDown(object sender, KeyEventArgs e)
        {
            ViewModel.BrowserFocused();
            var shortcut = TypeD.Models.Data.EditorShortcut.FromKey(
                e.Key.ToString(),
                e.KeyModifiers.HasFlag(KeyModifiers.Control),
                e.KeyModifiers.HasFlag(KeyModifiers.Shift),
                e.KeyModifiers.HasFlag(KeyModifiers.Alt),
                e.KeyModifiers.HasFlag(KeyModifiers.Meta));

            e.Handled = ViewModel.TryExecuteShortcut(shortcut);
        }
    }
}
