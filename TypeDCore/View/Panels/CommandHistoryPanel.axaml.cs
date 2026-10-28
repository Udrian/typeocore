using Avalonia.Controls;
using Avalonia.Interactivity;
using TypeDCore.ViewModel.Panels;

namespace TypeDCore.View.Panels
{
    public partial class CommandHistoryPanel : UserControl
    {
        // ViewModel
        CommandHistoryViewModel ViewModel { get; set; }

        public CommandHistoryPanel()
        {
            InitializeComponent();
            DataContext = ViewModel = new CommandHistoryViewModel(this);
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Undo();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Redo();
        }
    }
}
