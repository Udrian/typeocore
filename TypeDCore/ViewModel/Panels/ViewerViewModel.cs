using System;
using Avalonia.Controls;
using Avalonia.Threading;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Data.SettingContexts;
using TypeD.Models.Interfaces;
using TypeD.View.Viewer;
using TypeD.ViewModel;
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
        }

        // Functions
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
            }
        }

        public void Unload()
        {
            HookModel.RemoveHook<OpenComponentHook>(ComponentOpened);
            HookModel.RemoveHook<CloseComponentHook>(ComponentClosed);
            HookModel.RemoveHook<ComponentFocusHook>(ComponentFocus);
            HookModel.RemoveHook<BuildStatusChangedHook>(BuildStatusChanged);

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
