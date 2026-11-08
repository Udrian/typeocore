using System;
using TypeD.ViewModel;
using TypeD.Commands;
using TypeD.Models.Data;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Interfaces;
using TypeDCore.Components;
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
            var options = await ResourceModel.Get<IEditorDialogModel>().CreateComponentAsync(parameter.Project, parameter.Namespace, CoreComponent.SceneComponent());
            if (options != null)
            {
                var className = options.Name;
                var @namespace = options.Namespace;
                var parent = options.ParentComponent;
                var icon = options.Icon;
                var component = TypeDCoreProjectModel.CreateScene(
                    parameter.Project, className, @namespace, parent, icon);
                CommandHistoryModel.Record(
                    "Create scene type",
                    $"Created {component.FullName} based on {parent?.FullName ?? "an unspecified base type"}.",
                    () => ComponentProvider.Delete(parameter.Project, component),
                    () => component = TypeDCoreProjectModel.CreateScene(
                        parameter.Project, className, @namespace, parent, icon));
            }
        }
    }
}
