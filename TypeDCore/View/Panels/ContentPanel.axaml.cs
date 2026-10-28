using Avalonia.Controls;
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
}