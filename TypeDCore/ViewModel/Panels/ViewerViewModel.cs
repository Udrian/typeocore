using System.Windows.Controls;
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
        public void Unload()
        {
            HookModel.RemoveHook<OpenComponentHook>(ComponentOpened);
            HookModel.RemoveHook<CloseComponentHook>(ComponentClosed);
            HookModel.RemoveHook<ComponentFocusHook>(ComponentFocus);
        }

        public void TabSelectionChanged(IViewer viewer)
        {
            if (viewer == null)
                return;
            HookModel.Shoot(new ComponentFocusHook() { Project = Project, Component = viewer.Component});
        }

        void ComponentOpened(OpenComponentHook hook)
        {
            var setting = SettingModel.GetContext<MainWindowSettingContext>();

            IViewer viewer = PanelModel.CreateViewer(setting.ViewerType.Value);
            if (viewer == null) return;
            viewer.Init(hook.Project, hook.Component);

            ViewerPanel.Tabs.Items.Add(new TabItem() {
                Header = $"{hook.Component.ClassName}",
                Content = viewer
            });
        }

        void ComponentClosed(CloseComponentHook hook)
        {
            TabItem foundItem = null;
            foreach(var item in ViewerPanel.Tabs.Items)
            {
                if(((item as TabItem)?.Content as IViewer)?.Component?.FullName == hook.Component.FullName)
                {
                    foundItem = item as TabItem;
                    break;
                }    
            }
            ViewerPanel.Tabs.Items.Remove(foundItem);
        }

        void ComponentFocus(ComponentFocusHook hook)
        {
            TabItem foundItem = null;
            foreach (var item in ViewerPanel.Tabs.Items)
            {
                if (((item as TabItem)?.Content as IViewer)?.Component?.FullName == hook.Component.FullName)
                {
                    foundItem = item as TabItem;
                    break;
                }
            }
            if (foundItem != null)
                foundItem.IsSelected = true;
        }
    }
}
