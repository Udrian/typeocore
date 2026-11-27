using System;
using TypeD.Commands;
using TypeD.Models.Interfaces;
using TypeD.Models.Data;
using TypeDCore.Commands.Data;
using TypeDCore.Components;

namespace TypeDCore.Commands
{
    internal class CreateEntityTypeCommand : CustomCommand<CreateComponentCommandData>, IDisposable
    {
        private readonly IDisposable _registration;

        // Constructors
        public CreateEntityTypeCommand(Project project, IResourceModel resourceModel) : base(resourceModel)
        {
            _registration = resourceModel.Get<IEditorCommandRegistry>().Register(
                "component.create-entity",
                "Create entity type",
                "Create a new entity component type.",
                "global",
                string.Empty,
                this,
                () => new CreateComponentCommandData(project, null));
        }

        public void Dispose() => _registration.Dispose();

        public override async void Execute(CreateComponentCommandData parameter)
        {
            await CreateComponentTypeCommand.ExecuteAsync(parameter, CoreComponent.EntityComponent(), ResourceModel);
        }
    }
}
