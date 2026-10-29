using System;
using System.Collections.Generic;
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

            [ObservableProperty]
            private bool _isExpanded;

            [ObservableProperty]
            private bool _isRemoved;

            public ObservableCollection<Node> VisibleNodes { get; } = new ObservableCollection<Node>();

            private bool _isMatch = true;
            public bool IsMatch => _isMatch;

            public Node(Component component) : base()
            {
                Component = component;
                IsRemoved = component.Removed;
                Nodes = new ObservableCollection<Node>(Component.Children.Select(c => new Node(c)));
                foreach (var node in Nodes)
                    VisibleNodes.Add(node);
                UpdateTitle();
            }

            public bool ApplyFilter(string filter)
            {
                var childMatch = false;
                foreach (var child in Nodes)
                    childMatch |= child.ApplyFilter(filter);

                _isMatch = string.IsNullOrEmpty(filter) || MatchesFilter(filter);
                SyncVisible(VisibleNodes, Nodes.Where(n => n._isMatch || n.HasVisibleDescendants).ToList());
                if (!string.IsNullOrEmpty(filter) && VisibleNodes.Count > 0)
                    IsExpanded = true;
                return _isMatch || childMatch;
            }

            private bool HasVisibleDescendants => VisibleNodes.Count > 0;

            private bool MatchesFilter(string filter)
            {
                if (Component.FullName?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false)
                    return true;
                return Component.Properties.Any(p =>
                    (p.Name?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (p.Value?.ToString()?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            public static void SyncVisible(ObservableCollection<Node> target, List<Node> desired)
            {
                for (var i = target.Count - 1; i >= 0; i--)
                    if (!desired.Contains(target[i]))
                        target.RemoveAt(i);

                for (var i = 0; i < desired.Count; i++)
                {
                    if (i < target.Count && ReferenceEquals(target[i], desired[i]))
                        continue;
                    var existing = target.IndexOf(desired[i]);
                    if (existing >= 0)
                        target.Move(existing, i);
                    else
                        target.Insert(i, desired[i]);
                }
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
        public ObservableCollection<Node> VisibleNodes { get; } = new ObservableCollection<Node>();

        [ObservableProperty]
        private string _filterText = "";

        partial void OnFilterTextChanged(string value) => RefreshFilter();

        private void RefreshFilter()
        {
            var filter = FilterText?.Trim();
            foreach (var node in Nodes)
                node.ApplyFilter(filter);
            Node.SyncVisible(VisibleNodes, Nodes.Where(n => n.IsMatch || n.VisibleNodes.Count > 0).ToList());
        }

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
                        Nodes.Add(new Node(Component) { IsExpanded = true });
                        SelectedNode = Nodes[0];
                    }
                    RefreshFilter();
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
                    RefreshFilter();
            });

            HookModel.AddHook<ComponentAddedHook>((hook) =>
            {
                var node = Nodes.Flatten(n => n.Nodes).FirstOrDefault(n => ReferenceEquals(n.Component, hook.Parent));
                if (node != null)
                {
                    node.Nodes.Add(new Node(hook.Child));
                    RefreshFilter();
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
                        RefreshFilter();
                    }
                }
            });

            HookModel.AddHook<ComponentRemovedStateChangedHook>((hook) =>
            {
                var childNode = Nodes.Flatten(n => n.Nodes).FirstOrDefault(n => ReferenceEquals(n.Component, hook.Child));
                if (childNode != null)
                    childNode.IsRemoved = hook.Child.Removed;
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
                if (!string.IsNullOrWhiteSpace(FilterText))
                    RefreshFilter();
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
            if (nameProperty == null || nameProperty.ReadOnly || node.Component.ReadOnly)
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
