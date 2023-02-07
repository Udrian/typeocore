using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TypeD.Helpers;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.ViewModel;

namespace TypeDCore.ViewModel.Panels
{
    internal class ComponentViewModel : ViewModelBase
    {
        // Definitions
        public class Node : ViewModelBase
        {
            public Component Component { get; set; }

            public string Title { get => Component.ClassName; }
            public ObservableCollection<Node> Nodes { get => new ObservableCollection<Node>(Component.Children.Select(c => new Node(c))); }

            public Node(Component component)
            {
                Component = component;
            }
        }

        // Models
        public IHookModel HookModel { get; set; }

        // Data
        public Project LoadedProject { get; set; }

        // Properties
        private Component _component;
        public Component Component
        {
            get => _component;
            set
            {
                _component = value;
                Nodes.Clear();
                if (Component != null)
                {
                    Nodes.Add(new Node(_component));
                }
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Node> Nodes { get; set; }

        // Constructors
        public ComponentViewModel(FrameworkElement element, Project project) : base(element)
        {
            LoadedProject = project;
            HookModel = ResourceModel.Get<IHookModel>();
            Nodes = new ObservableCollection<Node>();

            HookModel.AddHook<ComponentFocusHook>((hook) =>
            {
                Component = hook.Component;
            });

            HookModel.AddHook<CloseComponentHook>((hook) =>
            {
                if(Component.FullName == hook.Component.FullName)
                {
                    Component = null;
                }
            });
        }

        // Functions
        public void ContextMenuOpened(ContextMenu contextMenu, Node node)
        {
            var componentContextMenuHook = new ComponentContextMenuHook()
            {
                Menu = new TypeD.View.Menu(),
                Project = LoadedProject,
                OpenedComponent = Component,
                SelectedComponent = node?.Component
            };
            HookModel.Shoot(componentContextMenuHook);

            contextMenu.Items.Clear();
            foreach (var menu in componentContextMenuHook.Menu.Items)
            {
                ViewHelper.InitMenu(contextMenu, menu, this);
            }
        }
    }
}
