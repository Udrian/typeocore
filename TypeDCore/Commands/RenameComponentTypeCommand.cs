using System;
using TypeD.Commands;
using TypeD.ViewModel;
using TypeD.Models.Data;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.View.Dialogs.Project;
using TypeDCore.Models.Interfaces;

namespace TypeDCore.Commands
{
    internal class RenameComponentTypeCommand : CustomCommand<ComponentCommandData>, IDisposable
    {
        // Providers
        IComponentProvider ComponentProvider { get; set; }
        ICommandHistoryModel CommandHistoryModel { get; set; }
        INodeSelectionModel NodeSelectionModel { get; set; }
        private readonly IDisposable Registration;

        // Constructors
        public RenameComponentTypeCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            ComponentProvider = ResourceModel.Get<IComponentProvider>();
            CommandHistoryModel = ResourceModel.Get<ICommandHistoryModel>();
            NodeSelectionModel = ResourceModel.Get<INodeSelectionModel>();
            Registration = ResourceModel.Get<IEditorCommandRegistry>().Register(
                "component.rename-type",
                "Rename component type",
                "Rename the specified component type.",
                "global",
                "F2",
                this,
                () => new ComponentCommandData
                {
                    Project = NodeSelectionModel.Project,
                    Component = NodeSelectionModel.SelectedComponentType
                });
        }

        public void Dispose() => Registration.Dispose();

        public override bool CanExecute(ComponentCommandData parameter)
        {
            return parameter?.Project != null
                && parameter.Component != null
                && NodeSelectionModel.IsComponentBrowserActive;
        }

        public override async void Execute(ComponentCommandData parameter)
        {
            var dialog = new RenameComponentTypeDialog(parameter.Component);
            if (await dialog.ShowDialog<bool>(ViewModelBase.MainWindow) == true)
            {
                var previousName = parameter.Component.FullName;
                var newName = $"{parameter.Component.Namespace}.{dialog.ViewModel.Name}";
                var previousClassName = parameter.Component.ClassName;
                var newClassName = dialog.ViewModel.Name;
                if (previousClassName == newClassName)
                    return;

                ComponentProvider.Rename(parameter.Project, parameter.Component, newClassName);
                CommandHistoryModel.Record(
                    "Rename component type",
                    $"Renamed {previousName} to {newName}.",
                    () => RenameComponent(parameter.Project, newName, previousClassName),
                    () => RenameComponent(parameter.Project, previousName, newClassName));
            }
        }

        private void RenameComponent(Project project, string currentFullName, string className)
        {
            var component = ComponentProvider.Load(project, currentFullName);
            if (component == null)
                throw new InvalidOperationException($"Could not find component type '{currentFullName}' to rename.");

            ComponentProvider.Rename(project, component, className);
        }
    }
}
