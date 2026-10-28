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
using TypeDCore.Commands.Data;
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

            [ObservableProperty]
            private string _editName;

            [ObservableProperty]
            private bool _isEditing;

            public void UpdateTitle() {
                Title = Component.Name;
            }

            public void BeginRename()
            {
                EditName = Component.Name;
                IsEditing = true;
            }

            public void CancelRename()
            {
                IsEditing = false;
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

        // Data
        public Project LoadedProject { get; set; }
        public Component SelectedComponent => NodeSelectionModel.SelectedNode;

        // Properties
        public Component Component => NodeSelectionModel.OpenedComponent;
        public ObservableCollection<Node> Nodes { get; set; }

        [ObservableProperty]
        private Node _selectedNode;

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
                    {
                        Nodes.Add(new Node(Component));
                        SelectedNode = Nodes[0];
                    }
                }
                else if (ReferenceEquals(hook.Project, LoadedProject)
                    && Component != null
                    && ReferenceEquals(GetRootComponent(hook.Component), Component))
                    SelectedNode = Nodes.Flatten(node => node.Nodes)
                        .FirstOrDefault(node => ReferenceEquals(node.Component, hook.Component));
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
            NodeSelectionModel.SetComponentExplorerActive();
            if (node != null && !ReferenceEquals(SelectedComponent, node.Component))
                SelectionChanged(node.Component);

            var componentContextMenuHook = CreateComponentContextMenu(node?.Component);

            contextMenu.Items.Clear();
            foreach (var menu in componentContextMenuHook.Menu.Items)
            {
                ViewHelper.InitMenu(contextMenu, menu, this);
            }
        }

        public bool TryExecuteShortcut(string shortcut, Component selectedComponent)
        {
            if (selectedComponent != null && !ReferenceEquals(SelectedComponent, selectedComponent))
                SelectionChanged(selectedComponent);
            else
                NodeSelectionModel.SetComponentExplorerActive();

            return EditorCommandRegistry.TryExecuteShortcut(shortcut, "global");
        }

        public void SelectionChanged(Component component)
        {
            NodeSelectionModel.SetComponentExplorerActive();
            HookModel.Shoot(new ComponentFocusHook() { Project = LoadedProject, Component = component, Root = false });
        }

        private static Component GetRootComponent(Component component)
        {
            while (component?.ParentComponent != null)
                component = component.ParentComponent;
            return component;
        }

        public void SetExplorerActive()
        {
            NodeSelectionModel.SetComponentExplorerActive();
        }

        public bool CommitRename(Node node)
        {
            var nameProperty = node.Component.Properties.FirstOrDefault(property => property.Name == "Name");
            if (nameProperty == null || nameProperty.ReadOnly)
            {
                node.CancelRename();
                return false;
            }

            var executed = EditorCommandRegistry.Execute("component.change-property", new ChangePropertyCommandData
            {
                Project = LoadedProject,
                Property = nameProperty,
                Value = node.EditName
            });
            if (executed)
                node.CancelRename();
            return executed;
        }

        public void BeginRename(Node node)
        {
            if (node == null || !ReferenceEquals(SelectedComponent, node.Component))
                return;

            NodeSelectionModel.SetComponentExplorerActive();
            node.BeginRename();
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

    }
}
