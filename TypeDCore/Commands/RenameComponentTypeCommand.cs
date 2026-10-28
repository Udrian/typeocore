using System;
using TypeD.Commands;
using TypeD.ViewModel;
using TypeD.Models.Data;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.View.Dialogs.Project;

namespace TypeDCore.Commands
{
    internal class RenameComponentTypeCommand : CustomCommand<ComponentCommandData>, IDisposable
    {
        // Providers
        IComponentProvider ComponentProvider { get; set; }
        ICommandHistoryModel CommandHistoryModel { get; set; }
        private readonly IDisposable _registration;

        // Constructors
        public RenameComponentTypeCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            ComponentProvider = ResourceModel.Get<IComponentProvider>();
            CommandHistoryModel = ResourceModel.Get<ICommandHistoryModel>();
            _registration = ResourceModel.Get<IEditorCommandRegistry>().Register(
                "component.rename-type",
                "Rename component type",
                "Rename the specified component type.",
                "global",
                string.Empty,
                this);
        }

        public void Dispose() => _registration.Dispose();

        public override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter);
        }

        public override bool CanExecute(ComponentCommandData parameter)
        {
            return base.CanExecute(parameter);
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
