using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TypeD.Helpers;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.ViewModel;

namespace TypeDCore.ViewModel.Panels
{
    internal static class FlattenExtension
    {
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f)
        {
            return e.SelectMany(c => f(c).Flatten(f)).Concat(e);
        }
    }

    internal partial class ComponentExplorerViewModel : ViewModelBase
    {
        // Definitions
        public partial class Node : ViewModelBase
        {
            public Component Component { get; private set; }

            [ObservableProperty]
            private string _title;
            public void UpdateTitle() {
                var nameProperty = Component.Properties.FirstOrDefault(p => p.Name == "Name");
                Title = string.IsNullOrEmpty(nameProperty?.Value as string) ? Component.ClassName : nameProperty.Value as string;
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
            }
        }
        public ObservableCollection<Node> Nodes { get; set; }

        // Constructors
        public ComponentExplorerViewModel(Control element, Project project) : base(element)
        {
            LoadedProject = project;
            HookModel = ResourceModel.Get<IHookModel>();
            Nodes = new ObservableCollection<Node>();

            HookModel.AddHook<ComponentFocusHook>((hook) =>
            {
                if(hook.Root)
                    Component = hook.Component;
            });

            HookModel.AddHook<CloseComponentHook>((hook) =>
            {
                if(Component.FullName == hook.Component.FullName)
                {
                    Component = null;
                }
            });

            HookModel.AddHook<ComponentAddedHook>((hook) =>
            {
                var node = Nodes.Flatten(n => n.Nodes).FirstOrDefault(n => n.Component.FullName == hook.Parent.FullName);
                if (node != null)
                {
                    node.Nodes.Add(new Node(hook.Child));
                }
            });

            HookModel.AddHook<PropertyChangedHook>((hook) =>
            {
                if (hook.Property.Name == "Name")
                {
                    var node = Nodes.Flatten(n => n.Nodes).FirstOrDefault(n => n.Component.FullName == hook.Component.FullName);
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

        public void SelectionChanged(Component component)
        {
            HookModel.Shoot(new ComponentFocusHook() { Project = LoadedProject, Component = component, Root = false });
        }
    }
}
