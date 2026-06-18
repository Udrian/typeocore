using Avalonia.Controls;
using Avalonia.Interactivity;
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
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        ViewModel.LoadAllContent();
    }
}