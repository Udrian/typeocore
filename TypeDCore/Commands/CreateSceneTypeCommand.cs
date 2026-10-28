using System;
using TypeD.ViewModel;
using TypeD.Commands;
using TypeD.Models.Data;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Interfaces;
using TypeDCore.View.Dialogs.Project;
using TypeD.Models.Providers.Interfaces;

namespace TypeDCore.Commands
{
    internal class CreateSceneTypeCommand : CustomCommand<CreateComponentCommandData>, IDisposable
    {
        // Models
        ITypeDCoreProjectModel TypeDCoreProjectModel { get; set; }
        IComponentProvider ComponentProvider { get; set; }
        ICommandHistoryModel CommandHistoryModel { get; set; }
        private readonly IDisposable _registration;

        // Constructors
        public CreateSceneTypeCommand(Project project, IResourceModel resourceModel) : base(resourceModel)
        {
            TypeDCoreProjectModel = ResourceModel.Get<ITypeDCoreProjectModel>();
            ComponentProvider = ResourceModel.Get<IComponentProvider>();
            CommandHistoryModel = ResourceModel.Get<ICommandHistoryModel>();
            _registration = ResourceModel.Get<IEditorCommandRegistry>().Register(
                "component.create-scene",
                "Create scene type",
                "Create a new scene component type.",
                "global",
                string.Empty,
                this,
                () => new CreateComponentCommandData(project, "Scenes"));
        }

        public void Dispose() => _registration.Dispose();

        public override async void Execute(CreateComponentCommandData parameter)
        {
            var dialog = new CreateSceneTypeDialog(parameter.Project, parameter.Namespace);
            if(await dialog.ShowDialog<bool>(ViewModelBase.MainWindow) == true)
            {
                var className = dialog.ViewModel.ComponentName;
                var @namespace = dialog.ViewModel.ComponentNamespace;
                var parent = dialog.ViewModel.ParentComponent;
                var component = TypeDCoreProjectModel.CreateScene(
                    parameter.Project, className, @namespace, parent);
                CommandHistoryModel.Record(
                    "Create scene type",
                    $"Created {component.FullName} based on {parent?.FullName ?? "an unspecified base type"}.",
                    () => ComponentProvider.Delete(parameter.Project, component),
                    () => component = TypeDCoreProjectModel.CreateScene(
                        parameter.Project, className, @namespace, parent));
            }
        }
    }
}
