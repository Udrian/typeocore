using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Linq;
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

        private void ComponentsTree_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = e.AddedItems.Cast<ComponentExplorerViewModel.Node>().FirstOrDefault();
            if (selectedItem != null)
            {
                ViewModel.SelectionChanged(selectedItem.Component);
            }
        }

        public void FocusTree()
        {
            Dispatcher.UIThread.Post(() =>
            {
                var item = ComponentsTree.GetSelfAndVisualDescendants()
                    .OfType<TreeViewItem>()
                    .FirstOrDefault(treeItem => treeItem.IsSelected);
                (item ?? (Control)ComponentsTree).Focus();
            }, DispatcherPriority.Background);
        }

        private void ComponentsTree_GotFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.SetExplorerActive();
        }

        private void ComponentsTree_KeyDown(object sender, KeyEventArgs e)
        {
            if (ComponentsTree.SelectedItem is not ComponentExplorerViewModel.Node selectedItem)
                return;
            if (selectedItem.IsEditing)
                return;

            if (e.Key == Key.F2)
            {
                ViewModel.BeginRename(selectedItem);
                Dispatcher.UIThread.Post(() => FocusNameEditor(selectedItem));
                e.Handled = true;
                return;
            }

            var shortcut = TypeD.Models.Data.EditorShortcut.FromKey(
                e.Key.ToString(),
                e.KeyModifiers.HasFlag(KeyModifiers.Control),
                e.KeyModifiers.HasFlag(KeyModifiers.Shift),
                e.KeyModifiers.HasFlag(KeyModifiers.Alt),
                e.KeyModifiers.HasFlag(KeyModifiers.Meta));

            e.Handled = ViewModel.TryExecuteShortcut(shortcut, selectedItem.Component);
        }

        private void NodeName_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox || textBox.DataContext is not ComponentExplorerViewModel.Node node)
                return;

            if (e.Key == Key.Enter)
            {
                ViewModel.CommitRename(node);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                node.CancelRename();
                e.Handled = true;
            }
        }

        private void FocusNameEditor(ComponentExplorerViewModel.Node node)
        {
            var item = ComponentsTree.GetSelfAndVisualDescendants()
                .OfType<TreeViewItem>()
                .FirstOrDefault(treeItem => ReferenceEquals(treeItem.DataContext, node));
            var textBox = item?.GetSelfAndVisualDescendants().OfType<TextBox>().FirstOrDefault();
            textBox?.Focus();
            textBox?.SelectAll();
        }

    }
}
