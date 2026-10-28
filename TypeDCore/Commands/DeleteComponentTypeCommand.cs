using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using TypeD.ViewModel;
using TypeDCore.Commands.Data;
using TypeD.Models.Providers.Interfaces;
using TypeD.Commands;
using TypeD.Models.Interfaces;
using TypeDCore.Models.Interfaces;

namespace TypeDCore.Commands
{
    internal class DeleteComponentTypeCommand : CustomCommand<ComponentCommandData>, IDisposable
    {
        // Providers
        IComponentProvider ComponentProvider { get; set; }
        ICommandHistoryModel CommandHistoryModel { get; set; }
        INodeSelectionModel NodeSelectionModel { get; set; }
        private readonly IDisposable _registration;

        // Constructors
        public DeleteComponentTypeCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            ComponentProvider = ResourceModel.Get<IComponentProvider>();
            CommandHistoryModel = ResourceModel.Get<ICommandHistoryModel>();
            NodeSelectionModel = ResourceModel.Get<INodeSelectionModel>();
            _registration = ResourceModel.Get<IEditorCommandRegistry>().Register(
                "component.delete-type",
                "Delete component type",
                "Delete the specified component type.",
                "global",
                "Delete",
                this,
                () => new ComponentCommandData
                {
                    Project = NodeSelectionModel.Project,
                    Component = NodeSelectionModel.SelectedComponentType
                });
        }

        public void Dispose() => _registration.Dispose();

        public override bool CanExecute(ComponentCommandData parameter)
        {
            return parameter?.Project != null
                && parameter.Component != null
                && NodeSelectionModel.IsComponentBrowserActive;
        }

        public override async void Execute(ComponentCommandData parameter)
        {
            var component = parameter.Component;
            var componentName = component.FullName;
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Delete Component Type",
                $"Delete component type '{componentName}'?",
                ButtonEnum.YesNoCancel,
                Icon.Question);
            if (await box.ShowWindowDialogAsync(ViewModelBase.MainWindow) != ButtonResult.Yes)
                return;

            ComponentProvider.Delete(parameter.Project, component);
            CommandHistoryModel.Record(
                "Delete component type",
                $"Deleted {componentName}.",
                () => ComponentProvider.Restore(parameter.Project, component),
                () => ComponentProvider.Delete(
                    parameter.Project,
                    ComponentProvider.Load(parameter.Project, componentName) ?? component));
        }
    }
}
