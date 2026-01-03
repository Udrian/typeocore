using Avalonia.Controls;
using Avalonia.Interactivity;
using TypeDCore.ViewModel.Panels;

namespace TypeDCore.View.Panels
{
    /// <summary>
    /// Interaction logic for OutputPanel.xaml
    /// </summary>
    public partial class OutputPanel : UserControl
    {
        // ViewModel
        OutputViewModel OutputViewModel { get; set; }

        // Constructors
        public OutputPanel()
        {
            InitializeComponent();

            OutputViewModel = new OutputViewModel(this);
            DataContext = OutputViewModel;
        }

        private void tbOutputText_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbOutputText.CaretIndex = int.MaxValue;
        }

        private void Panel_Unloaded(object sender, RoutedEventArgs e)
        {
            OutputViewModel.Unload();
        }
    }
}
