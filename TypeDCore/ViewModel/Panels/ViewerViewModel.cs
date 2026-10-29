using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using TypeD.Helpers;
using TypeDCore.Models.Data.Hooks;
using Avalonia.Threading;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Data.SettingContexts;
using TypeD.Models.Interfaces;
using TypeD.View.Viewer;
using TypeD.ViewModel;
using Avalonia.Input;
using TypeDCore.Models;
using TypeDCore.View.Panels;

namespace TypeDCore.ViewModel.Panels
{
    internal class ViewerViewModel : ViewModelBase
    {
        // Models
        IHookModel HookModel { get; set; }
        IPanelModel PanelModel { get; set; }
        ISettingModel SettingModel { get; set; }

        // Data
        ViewerPanel ViewerPanel { get; set; }
        Project Project { get; set; }
        IViewer Viewer { get; set; }
        Component ComponentToReload { get; set; }
        bool IsBuildInProgress { get; set; }

        // Constructors
        public ViewerViewModel(Project project, ViewerPanel viewerPanel) : base(viewerPanel)
        {
            ViewerPanel = viewerPanel;
            Project = project;

            HookModel = ResourceModel.Get<IHookModel>();
            PanelModel = ResourceModel.Get<IPanelModel>();
            SettingModel = ResourceModel.Get<ISettingModel>();

            HookModel.AddHook<OpenComponentHook>(ComponentOpened);
            HookModel.AddHook<CloseComponentHook>(ComponentClosed);
            HookModel.AddHook<ComponentFocusHook>(ComponentFocus);
            HookModel.AddHook<BuildStatusChangedHook>(BuildStatusChanged);
            HookModel.AddHook<ViewerContextMenuRequestedHook>(ContextMenuRequested);
        }

        // Functions
        void ContextMenuRequested(ViewerContextMenuRequestedHook hook)
        {
            if (hook.Project != Project || hook.Component == null)
                return;

            Dispatcher.UIThread.Post(() => ShowContextMenu(hook.Component));
        }

        void ShowContextMenu(Component component)
        {
            if (Viewer is not Control viewerControl)
                return;

            var menuHook = new ComponentContextMenuHook()
            {
                Menu = new TypeD.View.Menu(),
                Project = Project,
                OpenedComponent = Viewer.Component,
                SelectedComponent = component
            };
            HookModel.Shoot(menuHook);

            var contextMenu = new ContextMenu() { Placement = PlacementMode.Pointer };
            foreach (var menu in menuHook.Menu.Items)
            {
                ViewHelper.InitMenu(contextMenu, menu, this);
            }

            if (contextMenu.Items.Count > 0)
                contextMenu.Open(viewerControl);
        }

        public void Load()
        {
            if (Viewer == null)
            {
                var setting = SettingModel.GetContext<MainWindowSettingContext>();

                Viewer = PanelModel.CreateViewer(Project, setting.ViewerType.Value);
                var viewerControl = Viewer as Control;
                Grid.SetRow(viewerControl, 1);
                ViewerPanel.ViewerHost.Children.Add(viewerControl);

                Viewer.Init();
                AttachDrop(viewerControl);
            }
        }

        private ViewerDropHandler DropHandler { get; set; }

        void AttachDrop(Control viewerControl)
        {
            DropHandler = new ViewerDropHandler(ResourceModel);
            ViewerPanel.ViewerHost.SetValue(DragDrop.AllowDropProperty, true);
            ViewerPanel.ViewerHost.AddHandler(DragDrop.DragOverEvent, (_, e) =>
            {
                var canDrop = Viewer?.Component != null && !Viewer.Component.ReadOnly && DropHandler.CanDrop(Project, Viewer.Component);
                e.DragEffects = canDrop ? DragDropEffects.Copy : DragDropEffects.None;
                e.Handled = true;
            });
            ViewerPanel.ViewerHost.AddHandler(DragDrop.DropEvent, (_, e) =>
            {
                e.Handled = true;
                var root = Viewer?.Component;
                if (root == null || root.ReadOnly)
                    return;

                var position = e.GetPosition(viewerControl);
                DropHandler.Drop(Project, root, position.X, position.Y);
            });
        }

        public void Unload()
        {
            HookModel.RemoveHook<OpenComponentHook>(ComponentOpened);
            HookModel.RemoveHook<CloseComponentHook>(ComponentClosed);
            HookModel.RemoveHook<ComponentFocusHook>(ComponentFocus);
            HookModel.RemoveHook<BuildStatusChangedHook>(BuildStatusChanged);
            HookModel.RemoveHook<ViewerContextMenuRequestedHook>(ContextMenuRequested);

            if (Viewer != null)
            {
                Viewer.Unload();
            }
        }

        void ComponentOpened(OpenComponentHook hook)
        {
        }

        void ComponentClosed(CloseComponentHook hook)
        {
            if (ComponentToReload != null && ComponentToReload.FullName == hook.Component.FullName)
                ComponentToReload = null;

            if (Viewer != null)
            {
                Viewer.Unload();
            }
        }

        void ComponentFocus(ComponentFocusHook hook)
        {
            if (!hook.Root)
                return;

            if (IsBuildInProgress)
            {
                ComponentToReload = hook.Component;
                return;
            }

            if (Viewer != null)
            {
                Viewer.Load(hook.Component);
            }
        }

        void BuildStatusChanged(BuildStatusChangedHook hook)
        {
            if (hook.Project != Project)
                return;

            if (hook.IsBuilding)
            {
                Action beginBuild = () =>
                {
                    IsBuildInProgress = true;
                    if (Viewer?.Component is Component component)
                        ComponentToReload = component;
                };

                if (Dispatcher.UIThread.CheckAccess())
                    beginBuild();
                else
                    Dispatcher.UIThread.InvokeAsync(beginBuild).GetAwaiter().GetResult();
                return;
            }

            if (!hook.IsComplete)
                return;

            Dispatcher.UIThread.Post(() =>
            {
                IsBuildInProgress = false;
                var component = ComponentToReload;
                ComponentToReload = null;

                if (!hook.Succeeded || component == null || Viewer == null)
                    return;

                Viewer.Unload();
                Viewer.Load(component);
            });
        }
    }
}
