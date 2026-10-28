using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using System;
using TypeD.Models.Data;
using TypeD.ViewModel;
using TypeDCore.Models.Data;
using TypeDCore.Models.Interfaces;
using TypeDCore.ViewModel.Panels;

namespace TypeDCore.View.Panels
{
    /// <summary>
    /// Interaction logic for ViewerPanel.xaml
    /// </summary>
    public partial class ViewerPanel : UserControl
    {
        ViewerViewModel ViewerViewModel { get; set; }

        public ViewerPanel(Project project)
        {
            InitializeComponent();

            DataContext = ViewerViewModel = new ViewerViewModel(project, this);
        }

        private void ViewerPanelUnloaded(object sender, RoutedEventArgs e)
        {
            ViewerViewModel.Unload();
        }

        private void ViewerPanelLoaded(object sender, RoutedEventArgs e)
        {
            ViewerViewModel.Load();
        }
    }
}
