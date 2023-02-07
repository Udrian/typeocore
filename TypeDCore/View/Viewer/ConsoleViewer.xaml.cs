using System.Windows.Controls;
using TypeD.Models.Data;
using TypeD.View.Viewer;
using TypeDCore.ViewModel.Viewer;

namespace TypeDCore.View.Viewer
{
    /// <summary>
    /// Interaction logic for ConsoleViewer.xaml
    /// </summary>
    public partial class ConsoleViewer : UserControl, IViewer
    {
        // ViewModel
        ConsoleViewModel ConsoleViewModel { get; set; }

        // Properties
        public Component Component { get => ConsoleViewModel.Component; }

        // Constructors
        public ConsoleViewer()
        {
            InitializeComponent();

            DataContext = ConsoleViewModel = new ConsoleViewModel(this);
        }

        // Functions
        public void Init(Project project, Component component)
        {
            ConsoleViewModel.Init(project, component);
        }

        // Events
        private void ConsoleViewerUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ConsoleViewModel.Unload();
        }
    }
}
