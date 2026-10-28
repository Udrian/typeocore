using System;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeDCore.Models.Interfaces;

namespace TypeDCore.Models
{
    internal sealed class NodeSelectionModel : INodeSelectionModel, IDisposable
    {
        private IHookModel HookModel { get; set; }
        private readonly Action<ComponentFocusHook> ComponentFocusedHandler;
        private readonly Action<CloseComponentHook> ComponentClosedHandler;
        private readonly Action<ComponentRemovedHook> ComponentRemovedHandler;

        public Project Project { get; private set; }
        public Component OpenedComponent { get; private set; }
        public Component SelectedNode { get; private set; }

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
                SelectedNode = hook.Component;
        }

        private void OnComponentClosed(CloseComponentHook hook)
        {
            if (!ReferenceEquals(Project, hook.Project) || !ReferenceEquals(OpenedComponent, hook.Component))
                return;

            Project = null;
            OpenedComponent = null;
            SelectedNode = null;
        }

        private void OnComponentRemoved(ComponentRemovedHook hook)
        {
            if (SelectedNode != null && IsWithin(SelectedNode, hook.Child))
                SelectedNode = OpenedComponent;
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
