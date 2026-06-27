using Avalonia.Controls;
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
        }

        // Functions
        public void Load()
        {
            if (Viewer == null)
            {
                var setting = SettingModel.GetContext<MainWindowSettingContext>();

                Viewer = PanelModel.CreateViewer(Project, setting.ViewerType.Value);
                ViewerPanel.Tabs.Children.Add(Viewer as Control);

                Viewer.Init();
            }
        }

        public void Unload()
        {
            HookModel.RemoveHook<OpenComponentHook>(ComponentOpened);
            HookModel.RemoveHook<CloseComponentHook>(ComponentClosed);
            HookModel.RemoveHook<ComponentFocusHook>(ComponentFocus);

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
            if (Viewer != null)
            {
                Viewer.Unload();
            }
        }

        void ComponentFocus(ComponentFocusHook hook)
        {
            if (Viewer != null && hook.Root)
            {
                Viewer.Load(hook.Component);
            }
        }
    }
}
