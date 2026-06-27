using Avalonia.Controls;
using Avalonia.Interactivity;
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
        public Project Project { get; set; }

        // ViewModel
        ConsoleViewModel ConsoleViewModel { get; set; }

        // Properties
        public Component Component { get => ConsoleViewModel.Component; }

        // Constructors
        public ConsoleViewer()
        {
            InitializeComponent();

            DataContext = ConsoleViewModel = new ConsoleViewModel(Project, this);
        }

        // Functions
        public void Init()
        {
            ConsoleViewModel.Init();
        }

        public void Load(Component component)
        {
            ConsoleViewModel.Load(component);
        }

        public void Unload()
        {
            ConsoleViewModel.Unload();
        }

        // Events
        private void ConsoleViewerUnloaded(object sender, RoutedEventArgs e)
        {
            ConsoleViewModel.Unload();
        }
    }
}
