using Avalonia.Controls;
using TypeD.Models.Data;
using TypeDCore.ViewModel.Panels;

namespace TypeDCore.View.Panels
{
    /// <summary>Displays editable properties for components in a project.</summary>
    public partial class PropertiesPanel : UserControl
    {
        // ViewModel
        PropertiesViewModel ViewModel { get; set; }

        /// <summary>Creates a properties panel for the specified project.</summary>
        public PropertiesPanel(Project project)
        {
            InitializeComponent();
            DataContext = ViewModel = new PropertiesViewModel(this, project);
        }
    }
}