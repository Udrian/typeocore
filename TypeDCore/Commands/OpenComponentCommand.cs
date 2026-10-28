using System;
using System.Linq;
using TypeDCore.View.Panels;
using TypeD.Commands;
using TypeD.ViewModel;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.View.Dialogs.Project;
using TypeD.Models.Data;

namespace TypeDCore.Commands
{
    internal class OpenComponentCommand : CustomCommand<OpenComponentCommandData>, IDisposable
    {
        // Project
        Project Project { get; set; }

        // Models
        IComponentModel ComponentModel { get; set; }
        IEditorCommandRegistry EditorCommandRegistry { get; set; }
        private readonly IDisposable _registration;

        public OpenComponentCommand(Project project, IResourceModel resourceModel) : base(resourceModel)
        {
            Project = project;
            ComponentModel = ResourceModel.Get<IComponentModel>();
            EditorCommandRegistry = ResourceModel.Get<IEditorCommandRegistry>();

            _registration = EditorCommandRegistry.Register(
                "component.open",
                "Open component",
                "Open the selected component.",
                "global",
                "Ctrl+O",
                this,
                () => new OpenComponentCommandData { Project = Project });
        }

        public override async void Execute(OpenComponentCommandData parameter)
        {
            if(parameter.Component == null)
            {
                var componentSelectorDialog = new ComponentSelectorDialog(parameter.Project);
                await componentSelectorDialog.ShowDialog(ViewModelBase.MainWindow);
                parameter.Component = componentSelectorDialog.ViewModel.SelectedComponent;
            }

            if (parameter.Component == null) return;

            ComponentModel.Open(parameter.Project, parameter.Component);

            const string explorerId = "typed_componentexplorer";
            var panelModel = ResourceModel.Get<IPanelModel>();
            panelModel.OpenPanel(explorerId);
            (panelModel.GetPanels().FirstOrDefault(p => p.ID == explorerId)?.PanelView as ComponentExplorerPanel)?.FocusTree();
        }

        public void Dispose()
        {
            _registration.Dispose();
        }
    }
}
