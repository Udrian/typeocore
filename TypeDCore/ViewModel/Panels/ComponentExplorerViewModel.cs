using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using TypeD.Helpers;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.ViewModel;
using TypeDCore.Models.Interfaces;

namespace TypeDCore.ViewModel.Panels
{
    internal partial class ComponentExplorerViewModel : ViewModelBase
    {
        // Definitions
        public partial class Node : ViewModelBase
        {
            public Component Component { get; private set; }

            [ObservableProperty]
            private string _title;
            public void UpdateTitle() {
                Title = Component.Name;
            }

            [ObservableProperty]
            private ObservableCollection<Node> _nodes;

            public Node(Component component) : base()
            {
                Component = component;
                Nodes = new ObservableCollection<Node>(Component.Children.Select(c => new Node(c)));
                UpdateTitle();
            }
        }

        // Models
        public IHookModel HookModel { get; set; }
        private INodeSelectionModel NodeSelectionModel { get; set; }
        IEditorCommandRegistry EditorCommandRegistry { get; set; }
        IDisposable AddChildCommandRegistration { get; set; }

        // Data
        public Project LoadedProject { get; set; }
        public Component SelectedComponent => NodeSelectionModel.SelectedNode;

        // Properties
        public Component Component => NodeSelectionModel.OpenedComponent;
        public ObservableCollection<Node> Nodes { get; set; }

        // Constructors
        public ComponentExplorerViewModel(Control element, Project project) : base(element)
        {
            LoadedProject = project;
            HookModel = ResourceModel.Get<IHookModel>();
            NodeSelectionModel = ResourceModel.Get<INodeSelectionModel>();
            EditorCommandRegistry = ResourceModel.Get<IEditorCommandRegistry>();
            Nodes = new ObservableCollection<Node>();

            HookModel.AddHook<ComponentFocusHook>((hook) =>
            {
                if (hook.Root && ReferenceEquals(hook.Project, LoadedProject))
                {
                    Nodes.Clear();
                    if (Component != null)
                        Nodes.Add(new Node(Component));
                }
            });

            HookModel.AddHook<CloseComponentHook>((hook) =>
            {
                if (ReferenceEquals(hook.Project, LoadedProject) && NodeSelectionModel.OpenedComponent == null)
                    Nodes.Clear();
            });

            HookModel.AddHook<ComponentAddedHook>((hook) =>
            {
                var node = Nodes.Flatten(n => n.Nodes).FirstOrDefault(n => ReferenceEquals(n.Component, hook.Parent));
                if (node != null)
                {
                    node.Nodes.Add(new Node(hook.Child));
                }
            });
            
            HookModel.AddHook<ComponentRemovedHook>((hook) =>
            {
                var node = Nodes.Flatten(n => n.Nodes).FirstOrDefault(n => ReferenceEquals(n.Component, hook.Parent));
                if (node != null)
                {
                    var childNode = node.Nodes.FirstOrDefault(n => ReferenceEquals(n.Component, hook.Child));
                    if (childNode != null)
                    {
                        node.Nodes.Remove(childNode);
                    }
                }
            });

            HookModel.AddHook<PropertyChangedHook>((hook) =>
            {
                if (hook.Property.Name == "Name")
                {
                    var node = Nodes.Flatten(n => n.Nodes).FirstOrDefault(n => ReferenceEquals(n.Component, hook.Component));
                    if (node != null)
                    {
                        node.UpdateTitle();
                    }
                }
            });
        }

        // Functions
        public void ContextMenuOpened(ContextMenu contextMenu, Node node)
        {
            if (node != null && !ReferenceEquals(SelectedComponent, node.Component))
                SelectionChanged(node.Component);

            var componentContextMenuHook = CreateComponentContextMenu(node?.Component);

            contextMenu.Items.Clear();
            foreach (var menu in componentContextMenuHook.Menu.Items)
            {
                ViewHelper.InitMenu(contextMenu, menu, this);
            }
        }

        public void AddChildComponent(Component selectedComponent)
        {
            if (selectedComponent != null && !ReferenceEquals(SelectedComponent, selectedComponent))
                SelectionChanged(selectedComponent);

            var hook = CreateComponentContextMenu(selectedComponent);
            var addChildItem = hook.Menu.Items.FirstOrDefault(item => item.Name == "_Add Child Node");
            addChildItem?.Click?.Invoke(LoadedProject);
        }

        public bool TryExecuteShortcut(string shortcut, Component selectedComponent)
        {
            return EditorCommandRegistry.TryExecuteShortcut(shortcut, "component-explorer", selectedComponent);
        }

        public void UnregisterCommands()
        {
            AddChildCommandRegistration?.Dispose();
            AddChildCommandRegistration = null;
        }

        public void RegisterCommands()
        {
            RegisterAddChildCommand();
        }

        public void SelectionChanged(Component component)
        {
            HookModel.Shoot(new ComponentFocusHook() { Project = LoadedProject, Component = component, Root = false });
        }

        private ComponentContextMenuHook CreateComponentContextMenu(Component selectedComponent)
        {
            var hook = new ComponentContextMenuHook()
            {
                Menu = new TypeD.View.Menu(),
                Project = LoadedProject,
                OpenedComponent = Component,
                SelectedComponent = selectedComponent
            };
            HookModel.Shoot(hook);
            return hook;
        }

        private void RegisterAddChildCommand()
        {
            AddChildCommandRegistration?.Dispose();
            AddChildCommandRegistration = EditorCommandRegistry.Register(
                "node.add-child",
                "Add child node",
                "Add a child node to the selected node in the Component Explorer.",
                "component-explorer",
                "Ctrl+A",
                parameter => AddChildComponent(parameter as Component),
                parameter => parameter is Component,
                () => SelectedComponent);
        }
    }
}
