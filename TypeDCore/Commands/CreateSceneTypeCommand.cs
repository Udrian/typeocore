using System;
using TypeD.Commands;
using TypeD.Models.Data;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Components;

namespace TypeDCore.Commands
{
    internal class CreateSceneTypeCommand : CustomCommand<CreateComponentCommandData>, IDisposable
    {
        private readonly IDisposable _registration;

        // Constructors
        public CreateSceneTypeCommand(Project project, IResourceModel resourceModel) : base(resourceModel)
        {
            _registration = resourceModel.Get<IEditorCommandRegistry>().Register(
                "component.create-scene",
                "Create scene type",
                "Create a new scene component type.",
                "global",
                string.Empty,
                this,
                () => new CreateComponentCommandData(project, null));
        }

        public void Dispose() => _registration.Dispose();

        public override async void Execute(CreateComponentCommandData parameter)
        {
            await CreateComponentTypeCommand.ExecuteAsync(parameter, CoreComponent.SceneComponent(), ResourceModel);
        }
    }
}
