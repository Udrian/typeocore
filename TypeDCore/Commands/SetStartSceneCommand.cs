using System;
using TypeD.Commands;
using TypeD.Models.Data;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Interfaces;

namespace TypeDCore.Commands
{
    internal class SetStartSceneCommand : CustomCommand<ComponentCommandData>, IDisposable
    {
        // Models
        ITypeDCoreProjectModel TypeDCoreProjectModel { get; set; }
        ICommandHistoryModel CommandHistoryModel { get; set; }
        private readonly IDisposable _registration;

        // Constructors
        public SetStartSceneCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            TypeDCoreProjectModel = ResourceModel.Get<ITypeDCoreProjectModel>();
            CommandHistoryModel = ResourceModel.Get<ICommandHistoryModel>();
            _registration = ResourceModel.Get<IEditorCommandRegistry>().Register(
                "component.set-start-scene",
                "Set start scene",
                "Set the specified scene as the project's start scene.",
                "global",
                string.Empty,
                this);
        }

        public void Dispose() => _registration.Dispose();

        public override void Execute(ComponentCommandData parameter)
        {
            //var result = MessageBox.Show($"Do you want to set '{parameter.Component.FullName}' as start scene?", "Set Start Scene", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            //if (result == MessageBoxResult.Yes)
            {
                var previousStartScene = parameter.Project.StartScene;
                TypeDCoreProjectModel.SetStartScene(parameter.Project, parameter.Component);
                if (parameter.Project.StartScene != previousStartScene)
                {
                    var newStartScene = parameter.Project.StartScene;
                    CommandHistoryModel.Record(
                        "Set start scene",
                        $"Changed the start scene from {previousStartScene ?? "None"} to {newStartScene}.",
                        () => TypeDCoreProjectModel.SetStartScene(parameter.Project, previousStartScene),
                        () => TypeDCoreProjectModel.SetStartScene(parameter.Project, newStartScene));
                }
            }
        }
    }
}
