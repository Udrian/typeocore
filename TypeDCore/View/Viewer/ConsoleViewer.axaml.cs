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
        /// <summary>Gets or sets the project shown by this viewer.</summary>
        public Project Project { get; set; }

        // ViewModel
        ConsoleViewModel ConsoleViewModel { get; set; }

        // Properties
        /// <summary>Gets the component currently shown by the viewer.</summary>
        public Component Component { get => ConsoleViewModel.Component; }

        // Constructors
        /// <summary>Initializes a console viewer.</summary>
        public ConsoleViewer()
        {
            InitializeComponent();

            DataContext = ConsoleViewModel = new ConsoleViewModel(Project, this);
        }

        // Functions
        /// <summary>Initializes the viewer's console state.</summary>
        public void Init()
        {
            ConsoleViewModel.Init();
        }

        /// <summary>Loads the specified component into the viewer.</summary>
        public void Load(Component component)
        {
            ConsoleViewModel.Load(component);
        }

        /// <summary>Unloads the current component from the viewer.</summary>
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
