using Avalonia.Controls;
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

    }
}
