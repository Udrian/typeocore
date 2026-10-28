using System;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeDCore.Models.Interfaces;

namespace TypeDCore.Models
{
    internal sealed class NodeSelectionModel : INodeSelectionModel, IDisposable
    {
        private enum ActiveTree
        {
            None,
            ComponentExplorer,
            ComponentBrowser
        }

        private IHookModel HookModel { get; set; }
        private readonly Action<ComponentFocusHook> ComponentFocusedHandler;
        private readonly Action<CloseComponentHook> ComponentClosedHandler;
        private readonly Action<ComponentRemovedHook> ComponentRemovedHandler;

        public Project Project { get; private set; }
        public Component OpenedComponent { get; private set; }
        public Component SelectedNode { get; private set; }
        public Component SelectedComponentType { get; private set; }
        public bool IsComponentExplorerActive => _activeTree == ActiveTree.ComponentExplorer;
        public bool IsComponentBrowserActive => _activeTree == ActiveTree.ComponentBrowser;

        private ActiveTree _activeTree;

        public NodeSelectionModel()
        {
            ComponentFocusedHandler = OnComponentFocused;
            ComponentClosedHandler = OnComponentClosed;
            ComponentRemovedHandler = OnComponentRemoved;
        }

        public void Init(IResourceModel resourceModel)
        {
            HookModel = resourceModel.Get<IHookModel>();
            HookModel.AddHook(ComponentFocusedHandler);
            HookModel.AddHook(ComponentClosedHandler);
            HookModel.AddHook(ComponentRemovedHandler);
        }

        public void Dispose()
        {
            HookModel.RemoveHook(ComponentFocusedHandler);
            HookModel.RemoveHook(ComponentClosedHandler);
            HookModel.RemoveHook(ComponentRemovedHandler);
        }

        private void OnComponentFocused(ComponentFocusHook hook)
        {
            if (hook.Root)
            {
                Project = hook.Project;
                OpenedComponent = hook.Component;
                SelectedNode = hook.Component;
                return;
            }

            if (OpenedComponent == null || !ReferenceEquals(Project, hook.Project))
                return;

            if (ReferenceEquals(GetRootComponent(hook.Component), OpenedComponent))
            {
                SelectedNode = hook.Component;
                _activeTree = ActiveTree.ComponentExplorer;
            }
        }

        private void OnComponentClosed(CloseComponentHook hook)
        {
            if (!ReferenceEquals(Project, hook.Project) || !ReferenceEquals(OpenedComponent, hook.Component))
                return;

            Project = null;
            OpenedComponent = null;
            SelectedNode = null;
            _activeTree = ActiveTree.None;
        }

        private void OnComponentRemoved(ComponentRemovedHook hook)
        {
            if (SelectedNode != null && IsWithin(SelectedNode, hook.Child))
                SelectedNode = OpenedComponent;
        }

        public void SetComponentExplorerActive()
        {
            _activeTree = ActiveTree.ComponentExplorer;
        }

        public void SetNoTreeActive()
        {
            _activeTree = ActiveTree.None;
        }

        public void SetComponentBrowserActive(Project project, Component component)
        {
            Project = project;
            SelectedComponentType = component;
            _activeTree = ActiveTree.ComponentBrowser;
        }

        private static Component GetRootComponent(Component component)
        {
            while (component?.ParentComponent != null)
                component = component.ParentComponent;
            return component;
        }

        private static bool IsWithin(Component component, Component ancestor)
        {
            while (component != null)
            {
                if (ReferenceEquals(component, ancestor))
                    return true;
                component = component.ParentComponent;
            }
            return false;
        }
    }
}
