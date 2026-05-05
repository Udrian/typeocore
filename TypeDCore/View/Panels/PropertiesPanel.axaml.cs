using Avalonia.Controls;
using TypeD.Models.Data;
using TypeDCore.ViewModel.Panels;

namespace TypeDCore.View.Panels
{
    public partial class PropertiesPanel : UserControl
    {
        // ViewModel
        PropertiesViewModel ViewModel { get; set; }

        public PropertiesPanel(Project project)
        {
            InitializeComponent();
            DataContext = ViewModel = new PropertiesViewModel(this, project);
            ViewModel.RegisterTypeTemplates();
        }
    }
}