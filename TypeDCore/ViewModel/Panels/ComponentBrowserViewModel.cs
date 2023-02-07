using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TypeD.Helpers;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.ViewModel;
using TypeDCore.Commands;
using TypeDCore.Commands.Data;

namespace TypeDCore.ViewModel.Panels
{
    internal class ComponentBrowserViewModel : ViewModelBase
    {
        public class Node
        {
            public string IconPath { get { return $"/Icons/{Component}.png"; } }
            public string Name { get { return Context.Name; } }
            public string Key { get { return Context.Key; } }
            public string Component { 
                get {
                    var component = Context.Item as Component;
                    return component == null ? Context.Item.ToString() : component.TypeOBaseType.Name; 
                } 
            }
            public TypeD.View.TreeNodes.Node Context { get; set; }
            public ObservableCollection<Node> Nodes { get; set; }
        }

        // Models
        IHookModel HookModel { get; set; }

        // View
        TreeView TreeView { get; set; }

        // Data
        public Project LoadedProject { get; private set; }
        ObservableCollection<Node> Nodes { get; set; }

        // Commands
        public OpenComponentCommand OpenComponentCommand { get; set; }

        // Constructors
        public ComponentBrowserViewModel(FrameworkElement element, Project loadedProject, TreeView treeView) : base(element)
        {
            HookModel = ResourceModel.Get<IHookModel>();
            LoadedProject = loadedProject;
            TreeView = treeView;

            HookModel.AddHook<ComponentTreeBuiltHook>(BuildTree);
            Nodes = TreeToNodeList(LoadedProject.ComponentTree.Nodes);
            TreeView.ItemsSource = Nodes;

            OpenComponentCommand = new OpenComponentCommand(ResourceModel);
        }

        // Functions
        private ObservableCollection<Node> TreeToNodeList(IList<TypeD.View.TreeNodes.Node> treeNodes)
        {
            var nodes = new ObservableCollection<Node>();

            foreach(var treeNode in treeNodes)
            {
                nodes.Add(new Node()
                {
                    Context = treeNode,
                    Nodes = TreeToNodeList(treeNode.Nodes)
                });
            }

            return nodes;
        }

        public void DoubleClickItem(Node node)
        {
            var component = node.Context.Item as Component;
            if (component == null) return;
            OpenComponentCommand.Execute(new OpenComponentCommandData() { Project = LoadedProject, Component = component });
        }

        public void ContextMenuOpened(ContextMenu contextMenu, Node node)
        {
            var componentTypeBrowserContextMenuOpenedHook = new ComponentTypeBrowserContextMenuOpenedHook(node?.Context);
            HookModel.Shoot(componentTypeBrowserContextMenuOpenedHook);

            contextMenu.Items.Clear();
            foreach (var menu in componentTypeBrowserContextMenuOpenedHook.Menu.Items)
            {
                ViewHelper.InitMenu(contextMenu, menu, this);
            }
        }

        private void BuildTree(ComponentTreeBuiltHook hook)
        {
            TreeView.Dispatcher.Invoke(() =>
            {
                var treeNodes = TreeToNodeList(LoadedProject.ComponentTree.Nodes);
                Buildtree(treeNodes, Nodes);
            });
        }

        private void Buildtree(IList<Node> treeNodes, IList<Node> nodes)
        {
            var shouldBeDeleted = new List<Node>();
            var i = 0;
            foreach (var treeNode in treeNodes)
            {
                Node foundNode = null;
                foreach (var node in nodes)
                {
                    if (node.Key == treeNode.Key)
                    {
                        foundNode = node;
                        break;
                    }

                    var delFound = false;
                    foreach(var delTreeNode in treeNodes)
                    {
                        if(delTreeNode.Key == node.Key)
                        {
                            delFound = true;
                            break;
                        }
                    }
                    if(!delFound)
                    {
                        shouldBeDeleted.Add(node);
                    }
                }
                if (foundNode == null)
                {
                    nodes.Insert(i, treeNode);
                }
                else
                {
                    if(foundNode != treeNode)
                    {
                        nodes[i] = treeNode;
                    }
                    Buildtree(treeNode.Nodes, foundNode.Nodes);
                }
                i++;
            }
            foreach (var node in nodes)
            {
                var delFound = false;
                foreach (var delTreeNode in treeNodes)
                {
                    if (delTreeNode.Key == node.Key)
                    {
                        delFound = true;
                        break;
                    }
                }
                if (!delFound && !shouldBeDeleted.Contains(node))
                {
                    shouldBeDeleted.Add(node);
                }
            }
            foreach (var delNode in shouldBeDeleted)
            {
                nodes.Remove(delNode);
            }
        }
    }
}
