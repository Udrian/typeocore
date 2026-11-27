using System;
using TypeD.Commands;
using TypeD.Models.Data;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Components;

namespace TypeDCore.Commands
{
    internal class CreateDrawableTypeCommand : CustomCommand<CreateComponentCommandData>, IDisposable
    {
        private readonly IDisposable _registration;

        // Constructors
        public CreateDrawableTypeCommand(Project project, IResourceModel resourceModel) : base(resourceModel)
        {
            _registration = resourceModel.Get<IEditorCommandRegistry>().Register(
                "component.create-drawable",
                "Create drawable type",
                "Create a new drawable component type.",
                "global",
                string.Empty,
                this,
                () => new CreateComponentCommandData(project, null));
        }

        public void Dispose() => _registration.Dispose();

        public override async void Execute(CreateComponentCommandData parameter)
        {
            await CreateComponentTypeCommand.ExecuteAsync(parameter, CoreComponent.DrawableComponent(), ResourceModel);
        }
    }
}
