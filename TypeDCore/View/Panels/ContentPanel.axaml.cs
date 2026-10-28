using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using TypeD.Models.Data;
using TypeDCore.ViewModel.Panels;

namespace TypeDCore;

public partial class ContentPanel : UserControl
{
    // ViewModel
    ContentPanelViewModel ViewModel { get; set; }

    public ContentPanel(Project project)
    {
        DataContext = ViewModel = new ContentPanelViewModel(this, project);
        InitializeComponent();

        LoadContentAsync();
    }

    private async void LoadContentAsync()
    {
        await ViewModel.LoadAllContent();
    }

    private void ContentList_GotFocus(object sender, RoutedEventArgs e)
    {
        // Keeps global shortcuts like Delete from acting on the component tree while content is focused.
        ViewModel.SetNoTreeActive();
    }

    private void ContentList_KeyDown(object sender, KeyEventArgs e)
    {
        if (ViewModel.SelectedItem is not { IsEditing: false } item)
            return;

        if (e.Key == Key.F2)
        {
            BeginRename(item);
            e.Handled = true;
        }
        else if (e.Key == Key.Delete)
        {
            _ = ViewModel.Delete(item);
            e.Handled = true;
        }
    }

    private void ContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = ViewModel.SelectedItem == null;
    }

    private void Rename_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedItem != null)
            BeginRename(ViewModel.SelectedItem);
    }

    private void ShowInExplorer_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedItem != null)
            ViewModel.ShowInExplorer(ViewModel.SelectedItem);
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedItem != null)
            _ = ViewModel.Delete(ViewModel.SelectedItem);
    }

    private void BeginRename(ContentPanelViewModel.ImageItem item)
    {
        item.BeginRename();
        // The editor only exists in the visual tree once IsVisible has been applied.
        Dispatcher.UIThread.Post(() =>
        {
            var listItem = ContentList.GetSelfAndVisualDescendants()
                .OfType<ListBoxItem>()
                .FirstOrDefault(i => ReferenceEquals(i.DataContext, item));
            var textBox = listItem?.GetSelfAndVisualDescendants().OfType<TextBox>().FirstOrDefault();
            textBox?.Focus();
            textBox?.SelectAll();
        }, DispatcherPriority.Background);
    }

    private async void NameEditor_KeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox { DataContext: ContentPanelViewModel.ImageItem item })
            return;

        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            await ViewModel.CommitRename(item);
            ContentList.Focus();
        }
        else if (e.Key == Key.Escape)
        {
            item.CancelRename();
            e.Handled = true;
            ContentList.Focus();
        }
    }

    private void NameEditor_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox { DataContext: ContentPanelViewModel.ImageItem { IsEditing: true } item })
            item.CancelRename();
    }
}
