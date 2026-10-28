using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using TypeD.Models.Data;
using TypeDCore.ViewModel.Dialogs.Project;

namespace TypeDCore.View.Dialogs.Project
{
    /// <summary>
    /// Interaction logic for ComponentSelectorDialog.xaml
    /// </summary>
    public partial class ComponentSelectorDialog : Window
    {
        // ViewModel
        internal ComponentSelectorViewModel ViewModel { get; set; }

        // Constructors
        public ComponentSelectorDialog(TypeD.Models.Data.Project project)
        {
            InitializeComponent();
            DataContext = ViewModel = new ComponentSelectorViewModel(this, project);
            Opened += (_, _) =>
            {
                SelectFirst();
                tbFilter.Focus();
            };
            // Tunnel so the TextBox doesn't consume Up/Down first.
            tbFilter.AddHandler(KeyDownEvent, tbFilter_KeyDown, RoutingStrategies.Tunnel);
        }

        // Event Handlers
        private void tbFilter_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    MoveSelection(1);
                    e.Handled = true;
                    break;
                case Key.Up:
                    MoveSelection(-1);
                    e.Handled = true;
                    break;
                case Key.Enter:
                    if (lbComponents.SelectedItem is Component component)
                    {
                        ViewModel.SelectedComponent = component;
                        e.Handled = true;
                        Close(true);
                    }
                    break;
            }
        }

        private void SelectFirst()
        {
            if (ViewModel.FilteredComponents.Count > 0)
            {
                lbComponents.SelectedIndex = 0;
                lbComponents.ScrollIntoView(0);
            }
        }

        private void MoveSelection(int delta)
        {
            var count = ViewModel.FilteredComponents.Count;
            if (count == 0) return;

            var index = Math.Clamp(lbComponents.SelectedIndex + delta, 0, count - 1);
            lbComponents.SelectedIndex = index;
            lbComponents.ScrollIntoView(index);
        }

        private void lbComponents_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (lbComponents.SelectedItem is not Component component) return;
            ViewModel.SelectedComponent = component;
            Close(true);
        }

        private void tbFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.NameFilter.Filters = tbFilter.Text;
            ViewModel.UpdateFilter();
            SelectFirst();
        }
    }
}
