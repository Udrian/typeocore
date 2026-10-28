using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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

        private void ComponentsTree_KeyDown(object sender, KeyEventArgs e)
        {
            if (ComponentsTree.SelectedItem is not ComponentExplorerViewModel.Node selectedItem)
                return;

            var shortcut = TypeD.Models.Data.EditorShortcut.FromKey(
                e.Key.ToString(),
                e.KeyModifiers.HasFlag(KeyModifiers.Control),
                e.KeyModifiers.HasFlag(KeyModifiers.Shift),
                e.KeyModifiers.HasFlag(KeyModifiers.Alt),
                e.KeyModifiers.HasFlag(KeyModifiers.Meta));

            e.Handled = ViewModel.TryExecuteShortcut(shortcut, selectedItem.Component);
        }

    }
}
